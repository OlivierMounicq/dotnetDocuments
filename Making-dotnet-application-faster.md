# Making dotnet application faster

[Making dotnet application faster](https://app.pluralsight.com/library/courses/making-dotnet-applications-faster/table-of-contents)

## 1.Implementing Value Types correctly

### 1.1 Why Value Type ?

 * Reference type offer a set of managed services : locks, inheritance, and more
 * Values type : 
 	* you cannot use lock, inheritance and more! But 
  	* you could avoid the pressure on the garbage collector because instantiate on the stack

Additional difference between Reference type and Value type
* parameter passing :
	* value type : compare the content
	* reference type : 
* equality 


### 1.2 Object Layout

#### 1.2.1 Reference Type

Heap objects (reference type) have two header fields:


| Object Header Word     |
|:-----------------------|
| Method Table pointer   |
| First Object Field     |
| Second Object Field    |
 

__Object Header Word__
- 4 bytes long (32 bits) or 8 bytes long (64 bits)
- helps to synchronise mechanism (with lock statement) 
- GC maintains a list of free block 
- HashCode (if the dashcode has not been overriding) 

__Method Table Pointer__
- it helps the compiler to look up the method version and implement Polymorphism, the reflection service 


#### 1.2.2 Value Type

Value types (stack objects) don’t have headers. 
=> As the value type has not the header, so they cannot use the managed services.


Using the Value Types

Use value types when performance is critical:
* creating a large number of objects and keep then in the memory in the same time
* creating a large collection


For instance:

```cs
public struct / class Point2D
{
	public int X;
	public int Y;
}
```

Array of "class Point” => 10 000 000 objects = 320 000 000 bytes
Array of “struct Point” => 10 000 000 objects = 80 000 000 bytes

Memory reduction : 1/4 

![01](https://github.com/OMQ/CSharpNotes/blob/master/Performance/img/Making-dotnet-application-faster-01.png)


What is even worse : to access two continuous points need cache


### 1.3 Basic Value Type

The basic value type implementation is inadequate : if you use an array of Struct, we don’t enhance anymore the memory performance. But there are two operations that we could enhance: comparing and hashing object.

#### 1.3.1 Origin of Equals

``` List<T>.Contains ``` call Equals

Equal method
Declared by ```System.Object``` and overridden by ```System.ValueType```

```cs
//In System.ValueType, simplified version
public override bool Equals(object o)
{
 if(o == null) return false; //there is no point by executing this statement !
 if(o.GetType() != GetType()) return false; 
  if(CanCompareBits(this)) return FastEqualsCheck(this, o);
  foreach(FieldInfo f in GetType().GetFields()){
    if(!f.GetValue(this).Equals(f.GetValues(o)) return false;
  }
    
  return true;
}
```

#### 1.3.2 Boxing

Equal parameter must be boxing because the parameter of the Equals method is an object:

```cs
public virtual bool Equals(object o);
```

![01](https://github.com/OMQ/CSharpNotes/blob/master/Performance/img/Making-dotnet-application-faster-02.png)

#### 1.3.3 Avoiding Boxing and Reflection

- Override ```Equals```
- overload ```Equals```

```cs
struct Point2D : IEquatable<Point2D>
{
	public int X, Y;
	public override bool Equals(object o){...}
	public bool Equals(Point2D p){...}
}
```

#### 1.3.4 Final Tuning

##### 1.3.4.1 Add equality operators

```cs
struct Point2D
{
	public static bool operator==(Point2D a, Point2D b)...;
	public static bool operator!=(Point2D a, Point2D b)...;
}
```
You have to implement both operator (equal and not equal) otherwise the compiler will trigger an error.


##### 1.3.4.2 Add GetHashCode

* Used by ```Dictionary```, ```HashSet``` and other collections.
* Declared by ```System.Object```overriden by System.ValueType
* Must be consistent with ```Equals``` : A.Equals(B) => A.GetHashCode() == B.GetHashCode()

```cs
struct Point2D : IEquatable<Point2D>
{
  	public int X;
  	public int Y;
	
	public override int GetHashCode()
	{
		int hash = 19;
		hash = hash * 29 + X;
		hash = hash * 29 + Y;
		return hash;
	}
}
```

#### 1.3.5 The code

##### 1.3.5.1 First version: the worse performance

```cs
struct Point2D : IEquatable<Point2D>
{
  	public int X;
  	public int Y;
}
```

##### 1.3.5.2 Second version: the best performance

```cs
struct Point2D : IEquatable<Point2D>
{
  	public int X;
  	public int Y;
	
	public override bool Equals(object obj)
	{
		if(!(obj is Point2D)) return false;
		Point2D other = (Point2D)obj;
		return  X == other.X a&& Y == other.Y;
	}

	public bool Equals(Point2D other)
	{
		return X == other.X && Y == other.Y;
	}
}
```

## 2. Applying precompilation

### 2.1 Startup costs

#### 2.1.1 Cold Startup

Start the application for the first time since you boot your system.  
The common cost is the disk I/O : load assemblies, windows dll, data file (and after all assemblies/file are available in the cache for others applcations).  

#### 2.1.2 Warm Startup
You launch your application again after to close it.

* JIT Compilation (warming :  the cost)
* Signature validation
* DLL rebasing
* Initialization

### 2.2 Improving Startup Time with NGen
 
NGen precompiles .NET assemblies to native code
Ø  Ngen install MyApp.exe
* Includes dependencies
* Precompiled assemblies stored in C:\Windows\Assembly\NativeImages
* Fall back to original if stale

<u>Automatic NGen in Windows and CLR 4.5</u>
 
Enable by default in the windows services



### 2.3 Multi-Core Background JIT
 
* Usually, methods are compiled to native when invoke
* Multi-core background JIT in CLR 4.5
	* Opt in using System.Runtime.ProfileOPtimization class

```cs
Using System.Runtime;
 
ProfileOptimization.SetProfileRoot(folderName);
ProfileOptimization.StartProfileRoot(folderName);
``` 
 
A method already precompiled in another thread could be used directly without to use JIT to get the native code of the method
 
Relies on profile information generated at runtime : that information is used to determine which methods are likely to be invoked.
 
### 2.4 RuyJIT
 
A rewrite of the JIT Compiler
* Faster compilation (throughput)
* Better code (quality)
 
And it fixes some issues like pool code generation, ..etc.   
Relies on profile information collected at runtime
 
[performance improvements in ryujit in .net core and .net framework](https://blogs.msdn.microsoft.com/dotnet/2017/06/29/performance-improvements-in-ryujit-in-net-core-and-net-framework/)  
[github.com/dotnet/announcements/issues/10](https://github.com/dotnet/announcements/issues/10)

#### 2.4.1  How to set the JIT version

```bat
c:\program> set COMPLUS_AltJit = *
```

### 2.5 Managed Profile-Guided Optimization (MPGO)
 
Introduced in .NET 4.5
* Improves precompiled assemblies’ disk layout
* Places hot code and data closer together on disk 

### 2.6 Improving Cold Startup

* 2.6.1 I/O cost are &#35;1 thing to improve

#### 2.6.1 ILMerge (Microsoft research)

A.dll + B.dll + C.exe => ILMerge => Merged C.exe

#### 2.6.2 Executable packers

Example : Rugland Packer (RPX), available on CodePlex


NTFS File compression

#### 2.6.3 Placing strong-named assemblies in the GAC

The CLR has to verify the intergry of the dll (it has to calulate the hash code of each page). 
This step could be skipped if the dll is loaded from the GAC because the CLR considers the GAC like a trusted, secrure location.

#### 2.6.4 Windows SuperFetch

It prefetchs code and data by considering the previous application runnings.

Windows SuperFetch can not be disambled.

Windows SuperFetch is a windows service.

### 2.7 Precompiling Serialization Assemblies

#### 2.7.1 Serialization often creates dynamic methods on the first use

```cs
void Dynamic_Serialize_MyClass(MyClass c, BinaryWriter w)
{
	w.WriteInt32(c.X);
	w.WriteSingle(c.Y)
	w.WriteUInt16(c.Z);
}
```

#### 2.7.2 These methods can be precompiled

* __SGen.exe__ creates precompiled serialization assemblies for XmlSerializer.
* Protobuf-net has a precompilation tool


### 2.8 Precompiling Regexes

By default, the Regex class interprets the regular expression when you match it.  
Regex can generate IL code instead of using interpretation:

```cs
Regex r = new Regex(pattern, RegexOptions.Compiled);
```

Even better, you can precompile regular expression to an assembly:


```cs
var info = new RegexCompilationInfo(
	@"[0-9]+", RegexOptions.None,
	"Digits", "Utils", true);
Regex.CompileToAssembly(
	new[] { info }, new AssemblyName("RegexLib, ..."));
```

```Digits``` is the class name, ```Utils``` is the namespace

## 3. Using unsafe code and Pointers

### 3.1 Pointers

#### 3.1.1 Why to use pointers in C# ?

* Interoperability with Win32 and other DLLs
* Performance in specific scenarios

#### 3.1.2 Pointers and pinning

Accessing to an array:
* we want to go from ```byte[]``` to ```byte*``` 
* When getting a pointer to a heap object, what if the GC moves it during the compaction phase (in the SOH)? : actually, there will be a conflict : the GC change the adress memory of the object but it is not aware about the pointer which is an external reference for it.

So to use pointer, we have to pin the object:

```cs
byte[] source = ...;
fixed (byte* p = &source)
{
 ...
}
//In the following code, the source object won't be pinning
```

__Beware__ : by pinning the object, the GC won't be able to compact the SOH memory.

If there is an error during the pinning, the object won't be pinning and the keywork ```fixed``` acts as try/catch block.

#### 3.1.3 Directly manipulate memory

```cs
//copy the first element of the array
*p = (byte)12;

//cast of the first element into an INT
int x = *(int*)p;
```

With the pointer, yo can access to memeory zone out of the array: it requires ```unsafe``` block and "Allow unsafe code".

#### 3.1.4 Copying memory using pointers

* Mimicking ```Array.Copy``` or ```Buffer.BlockCopy```  
* Better to copy more than one byte per iteration

```cs
public static unsafe void Copy(byte[] src, byte[] dst)
{
	fixed (byte* p =&src[0]);
	fixed (byte* q =&dst[0]);
	{
		long* pSrc = (long*)p;
		long* pDst = (long*)q;

		for(int i = 0; i < dst.Length/8; i++)
		{
			*pDst = *pSrc;
			++pDst; ++pSrc;
		}
	}
}
```

* Might be interresting to unroll the loop


### 3.2 Reading Structure

#### 3.2.1 Marshal.PtrToStructure

* ```System.Runtime.InteropServices.Marshal``` is designed for the interopearbility scenarios
*  The method ```Marshal.PtrToStructure``` is useful to read structure from unmanaged memory.
	* ``` Object PtrToStructure(Type type, IntPtr address)``` 
* the GC can pin an object in memory and give us the pointer to it:

```xs
GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
try
{
	IntPtr address = handle.AddrOfPinnedObject();
}
finally
{
	//You are responsable about the object pinning
	handle.Free();
}
```

#### 3.2.2 Using pointers

* Pointers can help by casting 

```cs
fixed(byte* p = &data[offset])
{
	TcpHeader* pHeader = (TcpHeader*)p;
	return *pHeader;
}
```

#### 3.2.3 Generic approach

* Unfortunately, ```T*``` does not work : T must be blittable. 
* We can generate a method for each ```T``` and call it when necessary
	* Reflection.Emit
	* CSharpCodeProvider
	* Roslyn
	
## 4 Choosing a collection

### 4.1 Array

* Flat, sequential, statically sized 
* very fast to access to elements
* No per-element overhead
* Very efficient if the element type is a value type
* foundation for many other collection classes

Pros:
* accessing sequentially to element
* you know the size in advance
* you don't need to look up elements often

### 4.2 List&lt;T&gt;

* Dynamic (resizable) array
	* Double its size with each expansion
	* For 100 000 000 insertions : [log<sub>2</sub> 100 000 000] = 27 expansions
	* 
* Each time, there is an insertion into the list:
	* a new array is created by allocation
	* all elements of the former array are copied to the new array
	* the GC has to clean the former array used by the List class.
	
* Extension not at the end of the list is very expensive
	* good only to append data
* No specialization lookup facility
* Still no per-element overhead (because based on an array)

### 4.3 LinkedList&lt;T&gt;

* Doubly-linked list : each element knows its previous and next element
* Vey flexible collections for insertions/deletions : to insert/delete you only need to update the next and previous pointer.
* Still requires linear time (O(n)) for lookup
* Very big space overhead per element (each element has a next and previous pointers)

### 4.4 Trees

* ```SortedDictionary<K,V>``` and ```SortedSet<T>``` are implemented with a balanced binary search tree
	* efficient lookup by key
	* Sorted by key
	* The key has to implement the interface ```IComparable<T>``` 
* All fundamental operations take _O_(log<sub>2</sub>(n)) times
	* For example, log<sub>2</sub>(100 000 000) is less than 27
	* Great for storing dynamic data that is queried often
* Big space overhead per element (several addtional fields)	
* Trade-off between the memory (which is big) and the efficiency

### 4.5 Associative collection
 
 * ```Dictionary<K,V>``` and ```HashSet<T>``` use hashing to arrange the elements
 * Insertion, deletion and lookup work in _constant_ time _O_(1)
 	* GetHashCode must be well-distributed for this to happen
* Medium memory overhead
	* Combination of arrays and linked list
	* Smaller than trees in most cases
	

### 4.6 Comparison of Built-in Collections

| Collection | Space overhead | Lookup | Insertion | Deletion | Special |
|:-----------|:---------------|:-------|:----------|:---------|:--------|
| Array      | None           | _O_(n) | N/A       | N/A      |         |
| LinkedList | Medium         | _O_(n) | _O_(1)    | _O_(1)   |         |
| SortedDictionnary SortedSet | Very big | _O_(log<sub>2</sub>(n)) | _O_(log<sub>2</sub>(n)) | Sorted |
| Hashet Dictionary | Medium | _O_(1) | _O_(1) | _O_(1) | |


### 4.7 Scenarios

#### 4.7.1 Word frequency in a large body of text

* Best choice : ```Dictionary<K,V>``` 
* Second choice : ```SortDictionary<K,V>```

#### 4.7.2 Queue of orders in restaurant

* ```LinkedList<T>```

(The build-in queue uses an array.)

#### 4.7.3 Continuous process : buffer of continuous log

* ```List<T>```  : the element is appended at the end of the list

#### 4.7.4 Why custom collection ?

For instance : Find union algorithm



## 5. Make your code as parallel as necessary but no more

### 5.1 Defintions

#### 5.1.2 The concepts

* __Parallelism__ : running multiple threads in parallel
* __Concurrency__ : doing multiple things at once
* __Asynchrony__ : without blocking's the caller thread

#### 5.1.3 The workloads


* CPU-bound : data parallelism
* I/O-bound : I/O parallelism
* mixed

### 5.2 Data parallelism

* Parallelize operation on a collection of items
* TPL takes care of thread management

#### 5.2.1 Parallel loops

##### 5.2.1.1 Parallel.For

```cs
Parallel.For(from, to, i => {
	//loop body
	...;
});
```

##### 5.2.1.2 Parallel.ForEach

```cs
Parallel.ForEach(enumerable, element => {
	//loop body
	...;
});
```

##### 5.2.1.3 Customization

* breaking early
* limit parallelism
* Aggregation

### 5.3 I/O-Bound workloads and Asynchronous I/O

* data parallelism is suited for CPU-bound workloads
	* CPU are not good at sitting and waiting for I/O
* Asynchronous I/O operations
	* Asynchronous file read
	* Asynchronous HTTP POST
* Multiple outstanding I/O operations per thread

### 5.4 Asynchronous in C# 5: async and await

#### 5.4.1 async and await

```cs
async Task<Weather> GetWeatherAsync(string city)
{
	string url = Helpers.EncoreRequestUrl(city);
	string respose = await m_http.GetStringAsync(url);
	returns Helpers.ParseWeather(response);
}
```

#### 5.4.2 Awaiting task and IAsyncOperation

* await support 
	* The TPL class
	* IAsyncOperation Windows Runtime interface
	
```cs
//In System.Net.Http.HttpClient
public Task<string> GetStringAsync(string requestUri);

//In Windows.Web.Http.HttpClient
public IAsyncOperationWithProgresss<String, HttpProgress> GetStringAsync(Uri uri)
```

### 5.5 Parallelizing I/O requests

Start a few outstanding I/O operations and then...  

* __Wait-All__ : Process results when all operations are done :```Task.WhenAll(taskList)``` 
* __Wait-Any__ : Process each operation's result when available : ```Task.WhenAny(taskList)``` 

#### 5.5.1 Task.WhenAll

```cs
List<Task<string>> taskList = new List<Task<string>>[]{
  m_http.GetStringAsync(url1),
  m_http.GetStringAsync(url2),
  m_http.GetStringAsync(url3)  
};

Task<string[]> all = Task.WhenAll(taskList);
string[] results = await all;

//Process the result
```


#### 5.5.2 Task.WhenAny

```cs
List<Task<string>> taskList = new List<Task<string>>[]{
  m_http.GetStringAsync(url1),
  m_http.GetStringAsync(url2),
  m_http.GetStringAsync(url3)  
};

while(taskList.Count > 0)
{
  Task<Task<string>> any = Task.WhenAny(taskList);
  Task<string> completedTask = await any;
  //Process the result in the completedTask.Result
  taskList.Remove(completedTask);
}
```

### 5.6 Getting rid of locks

#### 5.6.1 Synchronization and Amdahl's law

* When using parallelism, shared resources require synchronization
* Amdahl's law 
  * if the fraction P of application requires synchronization, the maximum possible speedup is:
  
  
  * E.g, for P=0.5 (50%), the maximum speedup is 2x.

* Scalability is critical as the number of CPU increases.  

#### 5.6.2 Concurrent data structures

* Thread safe data structure in the TPL
  * ConcurrentDictionary (use lock in internally)
  * ConcurrentStack
  * ConcurrentBag
* ConcurrentStack and ConcurrentBag have several free-lock paths and use low level atomic synchronization primitive
* use them instead of a lock around the standard collections

#### 5.6.3 Aggregation

* Collect intermediate results into thread-local structure

```cs
Parallel.For(
    from,
    to,
    () => produce thread Local state,
    (i, _, local) => do work and return the new Local State,
    local => combine local states into global state
); 
```

#### 5.6.4 Lock-free operations 

* Atomic hardware primitives from the ```Interlocked``` class 
  * ```Interlocked.Increment```, ```Interlocked.Decrement```, ```Interlocked.Add``` .... 
* Especially useful : ```Interlocked.CompareExchange``` 

```cs
//Performs "shared * = x" atomically
static void AtomicMultiply(ref int shared, int x)
{
    int old, result
    do
    {
        old = shared;
	result = old * x;
    }
    while(old != Interlocked.CompareExchange(ref shared, old, result));	
}
```

#### 5.6.5 Example : get the prime numbers

##### 5.6.5.1 : Using lock keyword

```cs
public static List<uint> AllPrimesParallelWithLock(uint from, uint to)
{
    List<uint> result = new List<uint>();
    Parallel.For((int)from, (int)to, i => 
    {
        if(IsPrime((uint)i))
	{
	    lock(result)
	    {
	        result.Add((uint)i);
            }
        }	    
    });

    return result;	
}
```

##### 5.6.5.2 : Using aggregation based approach

```cs
public static List<uint> AllPrimesParallelAggregated(uint from, uint to)
{
    List<uint> result = new List<uint>();
    Parallel.For((int)from, (int)to, i => 
    {
        () => new List<uint>(), //Local state initializer
	(i, pls, local) =>
	{
	    if(IsPrime((uint)i))
	    {
	        local.Add((uint)i);
	    }
	    return local;	
	},
	local =>  //local to global state combinator
	{
	    lock(result)
	    {
	        result.AddRange(local);
	    }
	}
    });
    
    return result;	
}
```

## Summary

* Using value types correctly
* Applying precompilation
* using unsafe code and pointers
* choosing collection wisely
* Parallelism and asynchrony


## URL

[MPGO](https://eknowledger.wordpress.com/2012/04/27/clr-4-5-managed-profile-guided-optimization-mpgo/)
