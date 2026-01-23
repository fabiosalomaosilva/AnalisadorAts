namespace AnalisadorAts.Core.Models;

public class KeywordAnalysis
{
    public List<string> Missing { get; set; } = new();
    public List<string> Present { get; set; } = new();
    public List<string> Recommended { get; set; } = new();
}
