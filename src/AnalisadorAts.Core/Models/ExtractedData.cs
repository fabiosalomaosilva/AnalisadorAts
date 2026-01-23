namespace AnalisadorAts.Core.Models;

public class ExtractedData
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string EstimatedSeniority { get; set; } = "Não identificado";
    public List<string> Skills { get; set; } = new();
}
