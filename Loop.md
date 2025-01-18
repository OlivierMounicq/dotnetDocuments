#### [1] Arrays and for/foreach

The values of elements in this array won't be modified: _day_ is a complete value copy not just a reference copy:

```cs
public class Program
{
  public static void Main(string[] args)
  {
    string[] dayOfWeek = { 'Monday', 'Tuesday', 'Wenesday', 'Thursday', 'Friday', 'Saturday', 'Sunday' };
  
    for(int i = 0; i < dayOfWeek.Length; i++)
    {
        string day = dayOfWeeks[i];
        day = day + 's';
    }
  }
}


```
