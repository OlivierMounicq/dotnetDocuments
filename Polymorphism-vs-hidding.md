### Polymorphism vs hidding

The purpose of the polymorphism is to redefine the behavior of a instance's methode.  
The hidding method is just to hide the base class behavior.

##### Example

```cs 

public class BaseClass
{
  public [baseClassKeyword] void Method()
  {
    Console.WriteLine("I am the base class");
  }
}


public class InheritedClass : BaseClass
{
  public [inheritedClassKeyword] void Method()
  {
    Console.WriteLine("I am the inherited class");
  }
}
```

And we will replace [baseClassKeyword] and [inheritedClassKeyword] by different combinations using ```abstract```,
```override```, ```virtual```, ```new```.

#### The resume


|                                                             | Wrong                         |  abstract method                 | classical polymorphism            |        
|:------------------------------------------------------------|:------------------------------|:---------------------------------|:----------------------------------|
| Base class                                                  | ```public void Method```      | ```public abstract void Method```|```public virtual void Method()``` |                           
| Inherited class                                             | ```public override Method()```| ```public override Method()```   |```public override Method()```     | 
| Compilation                                                 | KO                            | OK                               |OK                                 |
| BaseClass a = new InheritedClass(); <br/>  a.Method();      |                               | I am the inherited class         | I am the inherited class          | 
| InheritedClass a = new InheritedClass(); <br/>  a.Method(); |                               | I am the inherited class         | I am the inherited class          |


|                                                             | Hide base method #1          | Hide base method #2               |Hide base method #3                    |         
|:------------------------------------------------------------|:-----------------------------|:----------------------------------|:--------------------------------------|
| Base class                                                  |```public void Method()```    | ```public virtual void Method()```|```public virtual void Method()```     |                            
| Inherited class                                             |```public new void Method()```|```public new void Method()```     |```public new virtual void Method()``` | 
| Compilation                                                 |OK                            |OK                                 |OK                                     |
| BaseClass a = new InheritedClass(); <br/>  a.Method();      |I am the base class           |I am the base class                |I am the base class                    |  
| InheritedClass a = new InheritedClass(); <br/>  a.Method(); |I am the inherited class      |I am the inherited class           |I am the inherited class               |
