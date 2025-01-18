#### [1] lock and Value Type

We cannot use a Value Type with the ```lock``` keyword because a value type does not contain a Sync Block Index.

```cs 
lock(<reference type>)
```

#### [2] The common multi threading issues

The main problems with the multi-threading are:
- the deadlock
- the livelock : [https://en.wikipedia.org/wiki/Deadlock#Livelock](en.wikipedia.org/wiki/Deadlock#Livelock)
- ABA problem : [https://en.wikipedia.org/wiki/ABA_problem](en.wikipedia.org/wiki/ABA_problem)

