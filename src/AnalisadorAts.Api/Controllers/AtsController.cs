using AnalisadorAts.Core.Interfaces;
using AnalisadorAts.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AnalisadorAts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtsController : ControllerBase
{
    private readonly IAnalysisService _analysisService;
    private readonly ILogger<AtsController> _logger;

    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".pdf", ".docx" };

    public AtsController(IAnalysisService analysisService, ILogger<AtsController> logger)
    {
        _analysisService = analysisService;
        _logger = logger;
    }

    /// <summary>
    /// Analisa um currículo de forma genérica, sem considerar uma vaga específica
    /// </summary>
    /// <param name="file">Arquivo PDF ou DOCX do currículo</param>
    /// <returns>Análise completa do currículo</returns>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeResume(IFormFile file)
    {
        try
        {
            // Validações
            var validation = ValidateFile(file);
            if (!validation.isValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Error = new ErrorDetail
                    {
                        Code = "INVALID_FILE",
                        Message = validation.message!
                    }
                });
            }

            _logger.LogInformation("Analisando currículo: {FileName}", file.FileName);

            using var stream = file.OpenReadStream();
            var result = await _analysisService.AnalyzeResumeAsync(stream, file.FileName);

            _logger.LogInformation("Análise concluída com score: {Score}", result.OverallScore);

            return Ok(result);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Formato de arquivo não suportado: {FileName}", file?.FileName);
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Error = new ErrorDetail
                {
                    Code = "UNSUPPORTED_FORMAT",
                    Message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar currículo: {FileName}", file?.FileName);
            return StatusCode(500, new ErrorResponse
            {
                Success = false,
                Error = new ErrorDetail
                {
                    Code = "PROCESSING_ERROR",
                    Message = "Erro ao processar o currículo. Por favor, tente novamente.",
                    Details = ex.Message
                }
            });
        }
    }

    /// <summary>
    /// Analisa um currículo comparando com os requisitos de uma vaga específica
    /// </summary>
    /// <param name="request">Arquivo do currículo e descrição da vaga</param>
    /// <returns>Análise de compatibilidade entre currículo e vaga</returns>
    [HttpPost("analyze-job")]
    [ProducesResponseType(typeof(JobMatchAnalysisResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AnalyzeResumeWithJob([FromForm] JobMatchRequest request)
    {
        try
        {
            // Validar arquivo
            var fileValidation = ValidateFile(request.File);
            if (!fileValidation.isValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Error = new ErrorDetail
                    {
                        Code = "INVALID_FILE",
                        Message = fileValidation.message!
                    }
                });
            }

            // Validar job description
            var jobValidation = ValidateJobDescription(request.JobDescription);
            if (!jobValidation.isValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Success = false,
                    Error = new ErrorDetail
                    {
                        Code = "INVALID_JOB_DESCRIPTION",
                        Message = jobValidation.message!
                    }
                });
            }

            _logger.LogInformation("Analisando currículo para vaga: {JobTitle}", request.JobDescription.Title);

            using var stream = request.File.OpenReadStream();
            var result = await _analysisService.AnalyzeResumeWithJobAsync(
                stream,
                request.File.FileName,
                request.JobDescription);

            _logger.LogInformation(
                "Análise concluída - Overall: {Overall}, Job Match: {JobMatch}",
                result.OverallScore,
                result.JobMatchScore);

            return Ok(result);
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Formato de arquivo não suportado: {FileName}", request.File?.FileName);
            return BadRequest(new ErrorResponse
            {
                Success = false,
                Error = new ErrorDetail
                {
                    Code = "UNSUPPORTED_FORMAT",
                    Message = ex.Message
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar análise de vaga: {FileName}", request.File?.FileName);
            return StatusCode(500, new ErrorResponse
            {
                Success = false,
                Error = new ErrorDetail
                {
                    Code = "PROCESSING_ERROR",
                    Message = "Erro ao processar a análise. Por favor, tente novamente.",
                    Details = ex.Message
                }
            });
        }
    }

    private (bool isValid, string? message) ValidateFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return (false, "Nenhum arquivo foi enviado");

        if (file.Length > MaxFileSize)
            return (false, $"Arquivo muito grande. Tamanho máximo: {MaxFileSize / 1024 / 1024}MB");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!AllowedExtensions.Contains(extension))
            return (false, $"Formato não suportado. Apenas PDF e DOCX são permitidos");

        return (true, null);
    }

    private (bool isValid, string? message) ValidateJobDescription(JobDescription? jobDescription)
    {
        if (jobDescription == null)
            return (false, "Descrição da vaga não foi fornecida");

        if (string.IsNullOrWhiteSpace(jobDescription.Title))
            return (false, "Título da vaga é obrigatório");

        if (jobDescription.RequiredSkills == null || !jobDescription.RequiredSkills.Any())
            return (false, "Skills obrigatórias são necessárias");

        return (true, null);
    }
}

public class JobMatchRequest
{
    public IFormFile File { get; set; } = null!;
    public JobDescription JobDescription { get; set; } = null!;
}

public class ErrorResponse
{
    public bool Success { get; set; }
    public ErrorDetail Error { get; set; } = null!;
}

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}
