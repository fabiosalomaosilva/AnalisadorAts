namespace AnalisadorAts.Core.Models;

public class JobMatchAnalysisResult : AnalysisResult
{
    public int JobMatchScore { get; set; }
    public JobRequirementsMatch JobRequirementsMatch { get; set; } = new();
}
