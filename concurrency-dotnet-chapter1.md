

### 1. The concepts of the functionnal programming

__Functionnal programming__
* abstraction + computation  
* avoiding the mutation of the data  
* _stateless_ : the output only depends upon input and not the state of the program at execution time  
* _expression_ : the functional programming treats computation as the evaluation of expressions.  

The main feature of the functional programming
* Immutability : removes the memory corruption  
* pure function : stateless function. A function always returns the same value for the same input parameters.  
* referential transparency : the returned value does not depend of the current state of other variables. To get the output value, you need the input parameters and the mapping between input paramater and output.
* lazy evaluation : the result is retrieved on-demand
* composability : you create higher-level abstraction by using simpler functions.
* null value not authorized 


* Before 2005 : single-core CPU
* A CPU single core performances stagnate

### 2. The functional programming concept

#### 2.1 The function composition

```csharp
static Func<A,> Compose<A,B,C>(this func<A,B> f, Func<B,C> g) 
    => (n) => g(f(n));
```    

#### 2.2 The closure

The _closures_ are a more convenient way to five functions access to a local state ad to pass data into background operations.

#### 2.3 The captured variable


#### 2.4 The memoization-cache

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


### The libraries
- Reactive Extension
- TPL (Microsoft)
- Intel's Threading Building Blocks (TBB°

#### Terminology

* _sequential programming_ : _it refers to a set of ordered instrcutions executed one at time on one CPU._
    * e.g : for the each person in line, the barrista is sequentially repeating the same set of instructions (grind coffee, brew coffee, steam milk, froth milk, and combine the coffee, and the milk to make a cappucino  
    * __*convenience*__ : a clear set of systematic instructions of what to do and when to do  
    * inconvenience : the barista can when during a task
* _concurrent programming_ : _it handles several operations at one time and does not require hardware support._ (running multiple tasks at the same time.) _Concurrency_ describes the ability to run several programs at the same time.
    * e.g : the barrista switches between the preparation of the coffee and preparing the milk.
* _parallel programming_ : executing multiples tasks simultaneously on several CPU. It achieves only in multicore devices. The goal is to maximize the use of all available computational resources.
    * e.g : one barrista prepares the coffee and another one prepares the milk
* _multitasking_ : concurrently performs multiple threads from different processes.
* _multithreading_ : extends the multitaskings concept for only one process.


    
__Mutual exclusion__
* lock => but it can lead to deadlock!

__Race condition__
* Race condition is a state that occurs when a shared mutable resource is accessed at the same time by multiple theads, leaving an inconsistent state.  
* Performance decline is a common problem when multiple threads contention that require synchronization.


__Divide and Conquer algorithm__  
! the multithreading applied to the _Quicksort_ algorithm (using a recursive function) could lead to worse performance => over-parallelization



  
  


