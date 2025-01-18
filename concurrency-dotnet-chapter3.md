## Chapter 3

####  Thread contention

A Thread contention is a condition where one thread is waiting for an object, being held by another thread, to be released.  The waiting thread cannot continue until the other thread releases the object (it’s locked).


#### System.Collection.Immutable

The namespace ```System.Collection.Immutable``` hase been added with the .NET 4.5 framework.  
The _immutable collection_ follows the functional paradigm.  

Any operations that change the data strctures don't modify the original instance. Instead, they return a changed copy and leave the original instance unchanged.

 The immutable collections have  been  heavily  tuned  for  maximum  performance  and  use  the  [Structural  Sharing pattern](https://en.wikipedia.org/wiki/Persistent_data_structure) to minimize garbage collector (GC) demands.
 
 ```cs
 var originalDictionary = new Dictionary<int,int>().ToImmutableDictionary();
 var modifiedCollection = originalDictionary.Add(key, value); //Return a new instance of the dictionary
 ```
 
 => Any changes to the collection in one thread are not visible to the other threads.
 
 
 #### Immutable collection
 
 | Immutable collection             | Mutable collection      |
 |----------------------------------|-------------------------|
 | ImmutableList<T>                 | List<t>                 |
 | ImmutableDictionary<TKey,TValue> | Dictionary<TKey,TValue> |
 | ImmutableHashSet<T>              | HashSet<T>              |
 | ImmutableStack<T>                | Stack<T>                | 
 | ImmutableQueue<T>                | Queue<T>                | 
  
  
 ####  Compare-And-Swap (CAS)
 
 The Compare-And-Swap operation is an atomic operation used in multithreaded programming.  
 The CAS instruction modifies shared data without the need to acquire and release a lock.
 
 - .NET method : ```Interlocked.CompareExchange```
 
 the ultimate combo : CAS + immutable shared data (no ABA problem)!
 
 
 #### The ImmutableInterlocked class
 
 You can use it to perform CAS operations on immutable collections.
 
 - namespace : System.Collections.Immutable
 
 #### Concurrent collection
 
 | Concurrent collection | Implementation details | Synchronization techniques |
 |-----------------------|------------------------|----------------------------|
 | ConcurrentBag<T>      | Works like a generic list | If multiple threads are detected, a primitive monitor coordinates their access =; otherwise the synchronization is avoided. |
 | ConcurrentStack<T>    | Generic stack implemented using a singly linked list | lock free using CAS technique |
 | ConcurrentQueue<T>    | Generic queue implemented using a linked list of array segments | Lock free using CAS technique |
 | ConcurrentDictionary<K,V> | Generic dictionary implemented using a hash table | Lock free for read operations, lock synchronization for updates.
 
 - the collections create an internal snapshot that mimics a temporary immutability to preserve thread safety during their iteration, allowing the snapshot to be enumerated safety
 - Those collections work well with algorithms implementing the producer/consummer pattern.
 
 
 
 
 
