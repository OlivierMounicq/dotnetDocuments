# Making dotnet application even faster

[Making dotnet application even faster](https://app.pluralsight.com/library/courses/making-dotnet-applications-even-faster/table-of-contents)

## 1. Garbage collection internals

### 1.1 Basic concepts

* Garbage collecton means we don't have to manually free memory

* Garbage collector is not free and has performance trade-off
  * questionable on real-time system, mobile device

* The CLR Garbage Collector is _almost-concurrent_, _parallel_, _compacting_, _mark-and-sweep_, _generational_, _tracing GC_.

### 1.2 Mark and sweep

There is three phases:
* Mark : identify all live objects
* Sweep : reclaim dead objects
* Compact : shift live objects together

Objects that can still be used must be kept alive

### 1.3 Roots

* Starting points for the Garbage Collector

The main roots are:
* Static variables
* local variable
* Finalization queue, f-reachable queue, GC handles ...
* Roots can ause memory leaks

```cs

internal class Program
{
    public static void TimerProcedure(object param)
    {
        Console.Clear();
        Console.WriteLine(DateTime.Now.TimeOfDay);
        
        GC.Collect();
    }

    static void Main(string[] rgs)
    {
        Console.Title = "Desktop clock";
        Console.SetWindowSize(20,2);
        Timer timer = new Timer(TimerProcedure, null, TimeSpan.Zero, TimeSpan.FromSecond(1));
        Console.ReadLine();
        timer.ToString(); //to avoid the frozen timer in the debug mode
        //Or
        GC.KeepAlive(timer);
    }
}
```

In the debug mode, the JIT compiler tells the GC that the local variable lifetime as to extended until the end of the methods.

The method ```GC.KeepAlive``` extends the local root's scope

### 1.4 GC Flavors

There are two 

### 1.4.1 Workstation GC

* There are multiple GC flavors
* Workstation GC is "kind of" suitable for client apps (WPF, console, service....)
  * The default for almost .net applications
  * unless the application runs inside ASP.NET
* GC runs on a single thread (concurrent or non concurrent)
* Concurrent workstation GC
  * Special GC thread : it is not triggered when the memory is full, it tries to optimize the garbage collection
  * Runs concurrently with application thread, only shot suspension
* Non-concurrent workstation GC:
  * No special GC thread
  * One of the app threads does the GC
  * All threads re suspended during GC
  
#### 1.4.2 Server GC

* One GC thread per logical processor, all working at once
* Separate heap area for each logical processor
* until the CLR 4.5, server GC was non-concurrent
* in CLR 4.5, server becomes concurrent
  * Now a reasonable default for many nigh-memory apps
 
#### 1.4.3 Configuration

* Configure preferred flavor in __app.config__
  * ignored if invalid
  * cannot switch flavors at runtime, but can query flavor by using ```GCSettings``` class.
  
  
```xml
<?xml version = "1.0" encoding="UTF-8" ?>
<configuration>
    <runtime>
        <gcServer enabled="true|false" />
        <gcConcurrent enabled="true|false" />
    </runtime>
</configuration>
```

### 1.5 Tools

Concurrency Visualizer

  
### 1.6 Generations

#### 1.6.1 Goals

* A full GC on entire heap is expensive and inefficient
* Divide the heap into regions nd perform smal  collections
* New objects die fast, old object stay alive
  * Typicall behavior for any applications
  * for instance : A web server
    * the object created for the query will die just after to sent the response
    * the object used for the initialization of server will stay alive for a while
 
#### 1.6.2 Characteristics

* The generations are larger on 64-bits system than the 32-bits system
* Generation sizes depend on the CPU cache size and even the amount of physical memory
* Make sure your temporary objects die young and avoid frequent promotions to generations 2

### 1.7 SOH / LOH

* Large objects are stored in a separate heap region (LOH)
  * _Large_ means larger than 85000 bytes or array of > 1000 doubles   

* The GC does not compact the LOH
  * this may cause fragmentation
* The LOH is considered art of generation 2 : the LOH objects are considered as old object.
  * Temporary large objects are a common GC performance problem.
* LOH fragmentation leads to waste of memory
  * .net 4.5.1 introduces LOH compaction
  
```cs
GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
GC.Collect();
```

  * You can test for LOH fragmentation using the ```!dumphead -stat``` SOS command

### 1.8 Foreground and background GC

* In concurrent GC, application threads continue to run during full GC
* What happens if an application thread allocates during the GC ?
  * In the CLR 2.0, the application thread waits for full GC to complete
  * In the CLR 4.0, the application thread launches a _foreground_ GC
  
### 1.9 Finalization

* The CLR runs a finalizer after the object becomes unreachable
* Let's design the finalization mechanism
  * Finalization queue for potentially "finalizable" objects
  * Identify candidates for finalization
  * Selecting thread for finalization : the finalizer thread
  * F-reachable queue for finalization candidates
  
* The finalization extends object lifetime
* The f-reachable queue might fill up faster than the finalizer thread can drain it





* the Finalize queue is a root



## 2. Garbage Collector and Performance counters

### 2.1 The metrics


### 2.2 Switching to value type

- 1. The value types are more friendly to the GC : the value types is always smaller than the reference type (Sync Block Index + Method Table Pointer)
- 2.The value types are embedded in their container
  - an array of struct is stored contiguously in memory
  - an array of reference type stores references ! so the array is not stored contiguously.
- Value types are easier for the GC to traverse  

### 2.3 Reducing allocation

- More allocations mean more work for GC
  - Allocating many large objects is especially bad
- PerfView can measure allocation sources and GC performance
- _Buffering_ can reduce small object allocation : ```StringBuilder``` vs ```String```
- Pooling can reduce large object allocation (cf [Object pool pattern](https://en.wikipedia.org/wiki/Object_pool_pattern))
  - e.g : WCF BufferManager

### 2.4 Finalization best practices

- keep your finalizers at the leaves of the object graph
- never block a finalizer
- make finalizable class very small
- beware of circular dependencies between the finalizable objects

## 3. Vectorizing CPU-Bound Algorithms

### 3.1 SIMD in Modern Processor

- Classic processors have instructions operating on scalar values
  - ```ADD EAX, DWORD PTR [ECX]```
  - Adds numbers, operates on processor registers
  - registers are small and fast on-chip memory locations that can be accessed much faster than the main memory
 
- Modern processors have instructions operating on vector values
  - one instruction can operate on multiple scalar elements (which is a vector)
  - SIMD = Single Instruction Multiple Data
  - SSE Introduce 16-byte instructions (x86 architecture)
  - AVX introduces 32-byte instructions (AVX stands dor Advanced Vector Extension)
 
### 3.2 More on SIMD instructions

- Instruction latency
  - ADD has a latency of 1 cycle and so does PADDD
  - Processors have specialized vector execution units

- Instruction throughput
  - Processors have a complex execution pipeline that affects throughput
  - ADD has throughput of 3 instructions per cycle
  - PADDD has throughput of 2 insttructions per cycle (but it does 4x the work)

- Conclsion : we should use vector instructions when possible
  - => But vectorizing algorithms is not always easy

### 3.3 SIMD Vector Ads

We would like to vectorize this loop:

```cs  
for(int i = 0; i< N; i++)
{  
	C[i] = A[i] + B[i]; //A,B C are arrays of int
} 
```  

- Add 4 or 8 integers in each iteration, requires 4x or 8x fewer iterations
  - No dependencies between iterations, sile compilers can even do this automatically

In this case, vectorization would be harder
  
```cs  
for(int i = 0; i< N; i++)
{  
	A[i] += A[i-1]; 
} 
```  

### 3.4 Microsoft.Bcl.Simd

- C&#35; does not have any special sauce for vector instructions
- Microsoft.Bcl.Simd and RuyJIT enable SIMD for C# code (2014)
  - If RyuJIT is not installed, the SIMD library falls back to scalar instructions
  - Currently, RuyJIT only supports x64

#### 3.4.1 New types

- ```Vector4f``` represents _four packed_ floating-point values
- for more hardware flexibility, use ```Vector<float>```, ```Vector<int>```, ```Vector<double>``` ...
  - ```Vector<T>.Length``` returns how many ```T```'s fit in a hardware SIMD register

Vectorize versio of array addition

```cs  
for(int i = 0; i< N; i+ = Vector<int>.Length)
{  
	Vector<int> vA = new Vector<int>(A,i);
	Vector<int> vB = new Vector<int>(B,i);
	Vector<int> vC = vA + vB;
	vc.Copy(C,i);
} 
```    

#### 3.4.2 Example

The former code:

```cs
public static void MultiplyScalar(int[] A, int M, int R, int[] B, int N, int[] C)
{
	for(int i = 0; i < M; i++)
	{
		for(int k = 0; k < R; ++k)
		{
			for(int j = 0; j < N; ++j)
			{
				C[i * M +j] += A[i * M + k] * B[k * R + j];
			}
		}
	]
}
```    


```cs
public static void MultiplyVector(int[] A, int M, int R, int[] B, int N, int[] C)
{
	int vecSize = Vector<int>.Length;
	Trace.Assert(N % vecSize == 0, "N must be divisible by the vector length");
	
	for(int i = 0; i < M; i++)
	{
		for(int k = 0; k < R; ++k)
		{
			for(int j = 0; j < N; j += vecsize)
			{
				Vector<int> vC = new Vector<int>(C, i * M + j);
				Vector<int> vB = new Vector<int>(B, k * R + j);
				Vector<int> vA = new Vector<int>(A[i * M + k]);
				vC += vA * vB;
				vC.CopyTo(C, i * M + j);
			}
		}
	]
}
``` 

#### 3.4.3 The configuration setting

Don't forget to set the environment variables

```bat
@echo off
SET COMPLUS_AltJit=*
SET COMPLUS_FeatureSIMD=1
``` 

### 3.5 Vectorizing Minimum-Maximum

How to vectorize that ?

```cs
int min = int.MaxValue;
int max = int.MinValue;

for(int i = 0; i < N; i++)
{
  min = Math.Min(min, A[i]);
  max = Math.Max(max, A[i]);
}
```

So apply that:


```cs
Vector<int> vmin  = new Vector<int>(int.MaxValue);
Vector<int> vmax  = new Vector<int>(int.MinValue);

for(int i = 0; i < N; i += Vector<int>.Length)
{
    Vector<int> va = new Vector<int>(A,i);
    
    Vector<int> vless = Vector.LessThan(va, vmin);
    vmin = Vector.ConditionalSelect(vless, va, vmin);
    
    Vector<int> vgrtr = Vector.GreaterThan(va, vmax);
    vmax = Vector.ConditionalSelect(vgrtr, va, vmax);
}



int min = int.MaxValue;
int max = int.MinValue;


for(int i = 0; i < Vector<int>.Length; ++i)
{
  min = Math.Min(min, vmin[i]);
  max = Math.Max(max, vmax[i]);
}
```



## 4. CPU Optimizations

### 4.1 Understanding the CPU

#### 4.1.1 Cache structure on modern processors

- Memory is slow, processors are fast
  - on Core i7, main memory access takes approximately 60ns (> 100 cycles)
  - Core i7 can do 2 vectors add instructions per cycle
  - One memory access oin the same time as > 200 vector add instructions
- This is the _memory wall_  
- Processors have caches a cache is a small block of fast memory
- Processors have caches that reduce the memory overhead
  - Multiple caches level (L1, L2, L3 and sometimes L4 on modern processors)
  
- Processor tries to get data from the cache L1, if it does not find the data it will ask to L2 cache otherwise it stop and so on.
- Each time the processor reads the main memory, it will bring the data inside the caches (L1, L2 and L3)
  - Each time, the processor reads the cache L3, it brings the data into the cache L2 and L1
  - Each time, the processor reads the cache L2, it brings the data into the cache L1

- Cache coherence with Windows :  it is impossible for a core to see a cached value that was modified by another core (even the modern architecture give the possibility to do that)


#### 4.1.2 Cache Coherence and Invalidation

- MESI cache coherence protocol
  - MESI : Modified, Exclusive, Shared, Invalid
- It is possible to share data unintentionally

### 4.2 Processor pipelines

- "Classic" processors execute instructios one after the other
  - e.g : 8086 processor
- Modern processors reorder instructions : the modern processors are happy to reorder instructons if they think it can help. This feature may cause some issues with the low-latency programming and lock-free algorithms.
- Modern processors have a deep execution pipeline which allows multiple instructions to execute at once
  - A processor has multiple executions units :
    - Vector floating-point | Memory Load | Arithmetic-Logical | Memory Store | Vector Integer
  - We can execute different kinds of operation in the same time : these units can run in parallel
  - Multiple instructions can execute in parallel
  - => And this is also why processor like to reorder and split instructions|
- ILP : Instruction-level parallelism
- ```ADD DWORD PTR[EAX], ECX``` reads memoryn performs add, stores result back into memory
  - Each tasks can be perform by an unit

| Memory Load           | Arithmetic-Logical         | Memory Store              | 
| read from address EAX | add value of ECX to result | store back to address EAX |  

- Modern processor actually have pipelines with up to 25 different stages

### 4.3 Data dependencies and Stalls

- An instruction might depend on the result of preceding instruction
- The processor can short-circuit around the dependency (forwarding)
- Specially, store-to-load forwarding
  - Very important optimization 
  - Easy to inhibit by stores and loads that are not the same size
  
### 4.4 Eliminating data dependencies

- Data dependency => it is hard for the processor to execute multiple instructions

```cs
int max = int.MinValue;
for(int i = 0; i < N; i++)
{
	max = Math.Max(max, A[i]); // A is an int[]
}
```

- To eliminate the dependencies, we can __unroll_ :

```cs
int max1 = int.MinValue;
int max2 = int.MinValue;

for(int i = 0; i < N; i += 2)
{
	max1 = Math.Max(max, A[i]);
	max2 = Math.Max(max, A[i+1]);
}
int max = Math.Max(max1, max2);
```

- up to 37% speedup 

    

## 5 - JIT Optimizations and .NET Native

### 5.1 JIT Optimizations

#### 5.1.1 JIT presentation

C&#35; Source => C&#35; Compiler => IL / MSIL

- JIT compiler compiles code on a method-by-method basis : 
  - the method must be called to be compiled
  - the method is called at the first time
- JIT Compilation can hurt startup times
- We can use PerfView to get a detailled report of JIT statistics of JIT
  - PerfView can produce detailed JIT time reports => JIT bottleneck ?
  - NGen.exe can precompile IL to native code before runtime (NGen stands for Native Image Generator)
  - JIT has been remplaced by RuyJIT
  
#### 5.1.2 Inlining

- Replace method call with method body:

Before

```cs  
int add(intx, int y)
{
	return x+y;
}

int z = add(5,3);
```

After

```cs  
int z = 5+3;
```

- Pros
  - Gets rid of method call overhead
  
- Cons
  - Code size grows
  - Debbuging is a little harder : you won't see all the calls in the call stack  
  
- It is important to inline very small methods (such as properties) which are called very often   
  - Verifying if a method was inlined is hard work


#### 5.1.3 Tuning inlining

JIT gives us a certain degree of control over whether and when inlining happens
We can inhibit or recommend inlining by using the ```MethodImpl``` attribute
 
The JIT will inline _small methods_  that :
- don't contain exception handling (try/catch)
- Are not virtual
- Are not recursive


To control inlining, use ```[MethodImpl]```  

```cs
[MethodImpl(MethodImplOptions.NonInlining)]
void Method1(){ }
```

You tell to JIT that it should try to inline the method
```cs
[MethodImpl(MethodImplOptions.AggressiveInlining)]
void Method2(){ }
```

#### 5.1.4 Array Bounds Check Elimination

- The JIT must ensure all array accesses are valid
  - checking this take a few cycle
  - the JIT can eliminate the check under som circumstances
  
- Especially important in tight, small loops that go from 0 to N
  - JIT tries to recognize this pattern and avoid the bounds check if possible  
  
- The JIT will eliminate the bounds check if:
  - the array reference is local : in this case, the array won't remplaced by another one during the runtime
  - The loop pattern is from 0 to ```array.Length``` and not the reverse. You should not skip elements and not start from something other than 0
 
```cs
int[] array = ....;

for(int i = 0; i < array.Lenght; i++)
{
	//use only array[i] to access the array here
}
``` 

### 5.2 .NET Native

#### 5.2.1 Compilation chain

- .NET Native ("Project N") produces  fully precompilled _native executable_ that does not depend n the JIT or the .NET Framework

__Standart .NET compilation pipeline__  
C&#35; source __=>__ C&#35; Compiler __=>__ IL __=>__ JIT Compiler __=>__ Native Code

__.NET Native compilation pipeline __  

C&#35; source __=>__ C&#35; Compiler __=>__ IL __=>__ ilc.exe __=>__ .ilexe __=>__ nutc_driver.exe __=>__ MDIL __=>__ rhbind.exe __=>__ Native Code

__ilc.exe__
- Marshaling : for the interuption (initially made by the CLR)
- sg.exe (serialization of the assemblies : JSON serialization, XML serialization, data contract serialization)
- Assembly merge (the assemblies are merges with also the .NET framework and only the methods used will be merged - any dead code and dead dependencies are eliminated
- Merge .Net Fx
- IL optimization
- nutc_driver.exe contains a C++ optimizing compiler. The goal is to produce a highly optimized machine code
- rhbind.exe : binds the MDIL binary and produces the final machine code


MDIL : Machine Dependent Intermediate Language

#### 5.2.2 .NET Native benefits

- Higher-quality compilation but slower compilation build time
- No dependency on .NET framework installation
  - a minimal CLR runtime is still present : mrt100_app.dll (mrt: minimal runtine)
- smaller memory footprint, faster startup

#### 5.2.2 .NET Native restriction

- only available to __Windows Store app__ : we cannot use .NET Native with WPF, ASP.NET, WCF Services...
- some "dynamic" APIs don't work
  - Reflection
  - Common in XAMl apps that use data binding
  - Dealt with using runtime directives (.rd.xml files)
  - many additonal directives
  - Serialization needs to be declared
