using AnalisadorAts.Core.Models;

namespace AnalisadorAts.Core.Interfaces;

public interface IAnalysisService
{
    Task<AnalysisResult> AnalyzeResumeAsync(Stream fileStream, string fileName);
    Task<JobMatchAnalysisResult> AnalyzeResumeWithJobAsync(Stream fileStream, string fileName, JobDescription jobDescription);
}
