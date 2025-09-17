namespace LoggerTest;

[AttributeUsage(AttributeTargets.Property)]
internal class FilterAtrribute : Attribute
{
    public string FilterSymbol { get; init; }

    public FilterAtrribute(string filterSymbol) 
        => FilterSymbol = filterSymbol;
}
