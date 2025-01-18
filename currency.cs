public class Currency
{
    public int Id{get;}
    public string Name{get;}
    public string Code {get;}

    public Currency(int id, string name, string code)
    {
        Id = id;
        Name = name;
        Code = code;
    }

    public override string ToString()
    {
        return $"{Id} - {Name} : {Code}";        
    }
}
