.

#### 1 The function composition

The classical way to aggrega


```csharp
Func <R,S> firstFunction = var1 => return doSometing(var1);

Func<S,T> secondFunction = var2 => return doOtherThing(var2);

S var2 = firstFunction(var1);
T result = secondFunction(var2);

//Or

var result = secondFunction(firstMethod(var1));

```    

But the best way is:

```csharp
static Func<R,T> Compose<R,S,T>(this func<R,S> firstFunction, Func<S,T> secondFunction) 
    => (n) => secondFunction(firstFunction(n));
    
//And then:
Func<R,T> doEverything = firstFunction.Compose(secondFunction);
R var1 = ....;
T result = doEverything(var1);
```


#### 2 The closure

##### 2.1 The purpose

The _closures_ are a more convenient way to five functions access to a local state ad to pass data into background operations.

```csharp
string closureVar = "I am a free variable dedicated to the closure";

Func<string,string> lambda = value => closureVar + " " + value;

```    


##### 2.2 The closure with the lambda expression in the multithreading context

In FP, closures are commonly used to manage mutable state to limit and isolate the scope of mutable structures, allowing thread-safe access.  
But to use the closure can lead to problem : 


```csharp
for(var i =0; i < 10; i++)
{
    Task.Factory.StartNew(() => Console.Writeline($"{i}");
}
```

But the output won't be equal to 0,1,2,3,4,5,6,7,8,9. Some threads can capture the same value.


#### 3 The memoization-cache

The goal is to store all results and their inputs in a cache to avoid to recompute each time the output value.

```csharp
static Func<T,R> Memoize<T,R>(Func<T,R> func)
   where T : IComparable
{
    Dictionary<T,R> cache = new Dictionary<T,R>();
    
    return arg => {
        if(cache.ContainsKey(arg))
            return cache(arg);
        return (cache[arg] = func(arg));    
    };
}
```

__Beware__ : the dictionary is unbounded : the items are only added never removed. To avoid the memory size issue, you can use the collection ``` ConditionalWeakDictionary ``` to add an automatic mechanism to remove the items for which the key has been removing by the GC.


#### 4 Lazy computation

##### 4.2 Lazy evaluation

Lazy evaluation : defering the evaluation until the last possible moment when it's accessed.

```Lazy<T>``` gives you the guarantée of full thread safety, regardless of the implementation of the function and ensures a single evaluation of the function

##### 4.2 Lazy evaluation vs eager evaluation 

- eager evaluation : strict evaluation (the expression is evaluated immediately)
- lazy evaluation : 

##### 4.3 Lazy evaluation and singleton

##### 4.3.1 Tread safe singleton

```csharp
public sealed class Singleton
{
    private static Singleton instance = null;
    private static readonly object lockObj = new object();
    
    private Singleton()
    {
        //....
    }

    public static Singleton Instance
    {
        get
        {
            lock(lockObj)
            {
                if(instance == null)
                {
                    instance = new Singleton();
                }
                return instance;            
            }        
        }
    }
}
```

##### 4.3.2 Tread safe singleton with double check

```csharp
public sealed class Singleton
{
    private static Singleton instance = null;
    private static readonly object lockObj = new object();
    
    private Singleton()
    {
        //....
    }

    public static Singleton Instance
    {
        get
        {
            if(instance == null)
                {
                lock(lockObj)
                {
                    if(instance == null)
                    {
                        instance = new Singleton();
                    }                    
                }            
            }
            return instance;
        }
    }
}
```


##### 4.3.3 Tread safe singleton with Lazy<T>

```csharp
public sealed class Singleton
{
    private static readonly Lazy<Singleton> = new Lazy<Singleton>(() => new Singleton());
    
    public static Singleton Instance
    {
        get
        {
            return lazy.Value;
        }
    }    
}
```

#### 5 The speculative computation

The speculative computation is performed before the actual algorithm and as soon as all the inputs of function are available.

__The goal__ : to amortize the cost of expensive computation and improve the performance.

__remark__ : this technic uses the closure to keep the result of the precompuation.


Each time you want to call this function, you have
- to get all parameters
- and wait the result of _DoSomethingForLongWhile_

```csharp
public static T DoSomething(R param1, S param2)
{
    var privateVar = DoSomethingForLongWhile(param1);
    
    return ComputeResult(privateVar, param2);
}
```

If you call several time the method DoSomething with the same value of param1, the privateVar is recomputed each time.

The better solution:

```csharp
public static Func<R,T> DoPartialSomething(R param1)
{
    var privateVar = DoSomethingForLongWhile(param1);
    
    return ComputeResult(privateVar, param2);
}

Func<S,T> fastComputation = DoPartialSomething(paramOne);

T result1 = fastComputation(paramTwo);
```











