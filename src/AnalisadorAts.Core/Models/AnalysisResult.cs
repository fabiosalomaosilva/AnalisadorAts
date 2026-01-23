namespace AnalisadorAts.Core.Models;

public class AnalysisResult
{
    public int OverallScore { get; set; }
    public int AtsCompatibilityScore { get; set; }
    public ExtractedData? ExtractedData { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<Suggestion> Suggestions { get; set; } = new();
    public KeywordAnalysis KeywordAnalysis { get; set; } = new();
    public string FormattingFeedback { get; set; } = string.Empty;
}
