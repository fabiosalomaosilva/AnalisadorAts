namespace AnalisadorAts.Core.Models;

public class JobDescription
{
    public string Title { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = new();
    public List<string> DesiredSkills { get; set; } = new();
    public int? MinimumExperience { get; set; }
    public string? Seniority { get; set; }
    public string? Description { get; set; }
}
