#### [1] Operator ++

```cs
int count = 1;
Console.WriteLine(100*count++);
Console.WriteLine(100*++count);

//Output
//100
//300
```

#### [2] Numeric operator on short

This code produce a compile time error : 

_error CS0266: Cannot implicitly convert type 'int' to 'short'. An explicit conversion exists (are you missing a cast?)_

```cs
short x = 1;
short y = 1;
short z;

z = x + y;
```

Actually, we have to cast the result return by the operator:

```cs
short x = 1;
short y = 1;
short z;
z = (short)(x + y);
```

#### [3] String and the operators == & !=

The operateurs __==__ and __!=__ have been defined to compare the value of the string.

```cs
string str = "TOTO";
string ing = "TOTO";
Console.WriteLine(str == ing);
```

#### [4] The override of the opertor == and !=

When you want to override an operator (== or !=), you have to override the other one.


