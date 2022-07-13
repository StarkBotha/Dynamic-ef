namespace DynamicEFprototype.Data;

public class DynamicSearchRequest
{
    public string Column { get; set; }

    public string SearchValue { get; set; }
    public string[] SearchValueArray { get; set; }

    public string Type { get; set; }
    public string ComparisonType { get; set; }
}