#### [1] Arrays and instantiation

This code won't compile because it uses an unassigned array:

```cs
public class Program
{
  public static void Main(string[] args)
  {
    int[] x;
    var copy = x; //compilation error
  
  }
}
