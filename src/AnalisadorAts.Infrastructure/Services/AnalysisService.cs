using AnalisadorAts.Core.Interfaces;
using AnalisadorAts.Core.Models;
using AnalisadorAts.Infrastructure.Parsers;
using AnalisadorAts.Infrastructure.TextProcessing;
using AnalisadorAts.Infrastructure.Extraction;

namespace AnalisadorAts.Infrastructure.Services;

public class AnalysisService : IAnalysisService
{
    private readonly DocumentParserFactory _parserFactory;
    private readonly DataExtractor _dataExtractor;
    private readonly SkillsDictionary _skillsDictionary;

    public AnalysisService(
        DocumentParserFactory parserFactory,
        DataExtractor dataExtractor,
        SkillsDictionary skillsDictionary)
    {
        _parserFactory = parserFactory;
        _dataExtractor = dataExtractor;
        _skillsDictionary = skillsDictionary;
    }

    public async Task<AnalysisResult> AnalyzeResumeAsync(Stream fileStream, string fileName)
    {
        // 1. Parse documento
        var parser = _parserFactory.GetParser(fileName);
        var rawText = await parser.ExtractTextAsync(fileStream, fileName);

        // 2. Normalizar texto
        var normalizedText = TextNormalizer.Normalize(rawText);

        // 3. Extrair dados
        var extractedData = _dataExtractor.Extract(rawText, normalizedText);

        // 4. Calcular scores
        var atsScore = CalculateAtsCompatibilityScore(rawText, normalizedText);
        var overallScore = CalculateOverallScore(extractedData, atsScore);

        // 5. Gerar análise
        var result = new AnalysisResult
        {
            OverallScore = overallScore,
            AtsCompatibilityScore = atsScore,
            ExtractedData = extractedData,
            KeywordAnalysis = new KeywordAnalysis
            {
                Present = extractedData.Skills,
                Recommended = _skillsDictionary.GetRecommendedSkills(extractedData.Skills)
            }
        };

        // 6. Adicionar feedback
        AddStrengthsAndWeaknesses(result, extractedData);
        AddSuggestions(result, extractedData);
        result.FormattingFeedback = GenerateFormattingFeedback(rawText, atsScore);

        return result;
    }

    public async Task<JobMatchAnalysisResult> AnalyzeResumeWithJobAsync(
        Stream fileStream,
        string fileName,
        JobDescription jobDescription)
    {
        // Primeiro fazer análise genérica
        var genericAnalysis = await AnalyzeResumeAsync(fileStream, fileName);

        // Criar resultado com matching de vaga
        var result = new JobMatchAnalysisResult
        {
            OverallScore = genericAnalysis.OverallScore,
            AtsCompatibilityScore = genericAnalysis.AtsCompatibilityScore,
            ExtractedData = genericAnalysis.ExtractedData,
            FormattingFeedback = genericAnalysis.FormattingFeedback
        };

        // Calcular match com a vaga
        var jobMatch = CalculateJobMatch(genericAnalysis.ExtractedData!, jobDescription);
        result.JobMatchScore = jobMatch.score;
        result.JobRequirementsMatch = jobMatch.requirements;

        // Análise de keywords específica para a vaga
        result.KeywordAnalysis = AnalyzeJobKeywords(
            genericAnalysis.ExtractedData!.Skills,
            jobDescription);

        // Recalcular overall score considerando o job match
        result.OverallScore = (int)((result.AtsCompatibilityScore * 0.3) +
                                     (result.JobMatchScore * 0.7));

        // Adicionar feedback específico da vaga
        AddJobSpecificFeedback(result, jobDescription);

        return result;
    }

    private int CalculateAtsCompatibilityScore(string rawText, string normalizedText)
    {
        var score = 100;
        var lines = rawText.Split('\n');
        var totalLines = lines.Length;

        // 1. Penalizar tabelas (detecção melhorada)
        var tableCharacters = new[] { "│", "┌", "└", "├", "┤", "─", "┬", "┴", "┼" };
        if (tableCharacters.Any(c => rawText.Contains(c)))
        {
            score -= 20;
        }

        // Detectar múltiplos pipes (|) na mesma linha
        var linesWithPipes = lines.Count(line => line.Count(c => c == '|') >= 2);
        if (linesWithPipes > 3)
        {
            score -= 15;
        }

        // 2. Penalizar múltiplas colunas (detecção melhorada)
        // Detectar linhas com muito espaçamento (tabs ou múltiplos espaços)
        var linesWithWideSpacing = lines.Count(line =>
        {
            // Contar sequências de 5+ espaços
            var spaceSequences = System.Text.RegularExpressions.Regex.Matches(line, @"\s{5,}").Count;
            return spaceSequences >= 2 || line.Contains("\t\t");
        });

        if (linesWithWideSpacing > totalLines * 0.15)
        {
            score -= 12;
        }

        // 3. Detectar uso excessivo de caracteres especiais (bordas, decorações)
        var specialCharsCount = rawText.Count(c => "═║╔╗╚╝╠╣╦╩╬▀▄█▌▐░▒▓■□▪▫".Contains(c));
        if (specialCharsCount > 10)
        {
            score -= 15;
        }

        // 4. Verificar informações básicas
        if (string.IsNullOrWhiteSpace(normalizedText))
        {
            score -= 30;
        }

        // 5. Penalizar se muito curto (menos de 200 caracteres)
        if (normalizedText.Length < 200)
        {
            score -= 20;
        }
        else if (normalizedText.Length < 500)
        {
            score -= 10;
        }

        // 6. Detectar muitas linhas vazias (formatação ruim)
        var emptyLines = lines.Count(string.IsNullOrWhiteSpace);
        if (emptyLines > totalLines * 0.4)
        {
            score -= 8;
        }

        // 7. Penalizar se tem muitos caracteres especiais em relação ao texto
        var alphaNumericCount = normalizedText.Count(char.IsLetterOrDigit);
        var specialInNormalized = normalizedText.Length - alphaNumericCount;
        if (alphaNumericCount > 0 && (specialInNormalized / (double)alphaNumericCount) > 0.3)
        {
            score -= 10;
        }

        return Math.Max(0, Math.Min(100, score));
    }

    private int CalculateOverallScore(ExtractedData data, int atsScore)
    {
        var score = atsScore * 0.3; // 30% ATS compatibility

        // 40% skills
        var skillsScore = Math.Min(100, data.Skills.Count * 6);
        score += skillsScore * 0.4;

        // 15% dados básicos
        var basicDataScore = 0;
        if (!string.IsNullOrEmpty(data.Name)) basicDataScore += 33;
        if (!string.IsNullOrEmpty(data.Email)) basicDataScore += 33;
        if (!string.IsNullOrEmpty(data.Phone)) basicDataScore += 34;
        score += basicDataScore * 0.15;

        // 15% senioridade identificada
        var seniorityScore = data.EstimatedSeniority != "Não identificado" ? 100 : 50;
        score += seniorityScore * 0.15;

        return (int)Math.Round(score);
    }

    private (int score, JobRequirementsMatch requirements) CalculateJobMatch(
        ExtractedData extractedData,
        JobDescription jobDescription)
    {
        var normalizedExtractedSkills = extractedData.Skills
            .Select(s => TextNormalizer.Normalize(s))
            .ToHashSet();

        var normalizedRequired = jobDescription.RequiredSkills
            .Select(s => TextNormalizer.Normalize(s))
            .ToList();

        var normalizedDesired = jobDescription.DesiredSkills
            .Select(s => TextNormalizer.Normalize(s))
            .ToList();

        var requiredMet = normalizedRequired.Count(req =>
            normalizedExtractedSkills.Any(skill => skill.Contains(req) || req.Contains(skill)));

        var desiredMet = normalizedDesired.Count(des =>
            normalizedExtractedSkills.Any(skill => skill.Contains(des) || des.Contains(skill)));

        var experienceMatch = CheckExperienceMatch(extractedData, jobDescription);

        var requirements = new JobRequirementsMatch
        {
            RequiredSkillsMet = requiredMet,
            RequiredSkillsTotal = normalizedRequired.Count,
            DesiredSkillsMet = desiredMet,
            DesiredSkillsTotal = normalizedDesired.Count,
            ExperienceMatch = experienceMatch
        };

        // Calcular score
        var requiredScore = normalizedRequired.Count > 0
            ? (requiredMet / (double)normalizedRequired.Count) * 60
            : 60;

        var desiredScore = normalizedDesired.Count > 0
            ? (desiredMet / (double)normalizedDesired.Count) * 30
            : 30;

        var experienceScore = experienceMatch ? 10 : 0;

        var totalScore = (int)(requiredScore + desiredScore + experienceScore);

        return (totalScore, requirements);
    }

    private bool CheckExperienceMatch(ExtractedData extractedData, JobDescription jobDescription)
    {
        if (jobDescription.MinimumExperience == null && string.IsNullOrEmpty(jobDescription.Seniority))
            return true;

        if (!string.IsNullOrEmpty(jobDescription.Seniority))
        {
            var requiredSeniority = jobDescription.Seniority.ToLower();
            var extractedSeniority = extractedData.EstimatedSeniority.ToLower();

            var seniorityLevels = new Dictionary<string, int>
            {
                ["junior"] = 1,
                ["júnior"] = 1,
                ["pleno"] = 2,
                ["senior"] = 3,
                ["sênior"] = 3
            };

            if (seniorityLevels.TryGetValue(extractedSeniority, out var extractedLevel) &&
                seniorityLevels.TryGetValue(requiredSeniority, out var requiredLevel))
            {
                return extractedLevel >= requiredLevel;
            }
        }

        return true;
    }

    private KeywordAnalysis AnalyzeJobKeywords(List<string> extractedSkills, JobDescription jobDescription)
    {
        var normalizedExtracted = extractedSkills.Select(s => TextNormalizer.Normalize(s)).ToHashSet();
        var allRequiredAndDesired = jobDescription.RequiredSkills
            .Concat(jobDescription.DesiredSkills)
            .Select(s => TextNormalizer.Normalize(s))
            .ToList();

        var present = extractedSkills.Where(skill =>
        {
            var normalized = TextNormalizer.Normalize(skill);
            return allRequiredAndDesired.Any(req =>
                normalized.Contains(req) || req.Contains(normalized));
        }).ToList();

        var missing = jobDescription.RequiredSkills
            .Concat(jobDescription.DesiredSkills)
            .Where(skill =>
            {
                var normalized = TextNormalizer.Normalize(skill);
                return !normalizedExtracted.Any(ext =>
                    ext.Contains(normalized) || normalized.Contains(ext));
            })
            .Distinct()
            .ToList();

        var recommended = _skillsDictionary.GetRecommendedSkills(extractedSkills)
            .Where(r => !missing.Contains(r))
            .Take(3)
            .ToList();

        return new KeywordAnalysis
        {
            Present = present,
            Missing = missing,
            Recommended = recommended
        };
    }

    private void AddStrengthsAndWeaknesses(AnalysisResult result, ExtractedData data)
    {
        // Pontos fortes
        if (data.Skills.Count >= 10)
            result.Strengths.Add("Ampla variedade de skills técnicas identificadas");

        if (data.EstimatedSeniority != "Não identificado")
            result.Strengths.Add($"Senioridade claramente identificada como {data.EstimatedSeniority}");

        if (!string.IsNullOrEmpty(data.Email) && !string.IsNullOrEmpty(data.Phone))
            result.Strengths.Add("Informações de contato completas");

        if (result.AtsCompatibilityScore >= 80)
            result.Strengths.Add("Currículo bem estruturado e ATS-friendly");

        // Pontos fracos
        if (data.Skills.Count < 5)
            result.Weaknesses.Add("Poucas skills técnicas identificadas");

        if (string.IsNullOrEmpty(data.Email))
            result.Weaknesses.Add("E-mail não identificado");

        if (data.EstimatedSeniority == "Não identificado")
            result.Weaknesses.Add("Nível de senioridade não está claro");

        if (result.AtsCompatibilityScore < 70)
            result.Weaknesses.Add("Formatação pode prejudicar leitura por sistemas ATS");
    }

    private void AddSuggestions(AnalysisResult result, ExtractedData data)
    {
        if (data.Skills.Count < 8)
        {
            result.Suggestions.Add(new Suggestion
            {
                Title = "Adicionar mais skills técnicas",
                Description = "Inclua todas as tecnologias, frameworks e ferramentas que você domina para aumentar a compatibilidade com vagas"
            });
        }

        if (string.IsNullOrEmpty(data.Email))
        {
            result.Suggestions.Add(new Suggestion
            {
                Title = "Adicionar e-mail de contato",
                Description = "Certifique-se de incluir um e-mail profissional no início do currículo"
            });
        }

        if (result.AtsCompatibilityScore < 80)
        {
            result.Suggestions.Add(new Suggestion
            {
                Title = "Melhorar formatação ATS",
                Description = "Evite tabelas, múltiplas colunas e formatações complexas. Use uma estrutura simples e linear"
            });
        }

        result.Suggestions.Add(new Suggestion
        {
            Title = "Adicionar métricas e resultados",
            Description = "Inclua números concretos sobre impacto dos projetos (ex: 'Reduziu tempo de processamento em 40%', 'Liderou equipe de 5 desenvolvedores')"
        });
    }

    private void AddJobSpecificFeedback(JobMatchAnalysisResult result, JobDescription jobDescription)
    {
        result.Strengths.Clear();
        result.Weaknesses.Clear();
        result.Suggestions.Clear();

        var match = result.JobRequirementsMatch;

        // Pontos fortes
        if (match.RequiredSkillsMet == match.RequiredSkillsTotal)
            result.Strengths.Add("Possui todas as skills obrigatórias da vaga");
        else if (match.RequiredSkillsMet >= match.RequiredSkillsTotal * 0.7)
            result.Strengths.Add("Possui a maioria das skills obrigatórias da vaga");

        if (match.ExperienceMatch)
            result.Strengths.Add("Senioridade compatível com o requisitado");

        if (match.DesiredSkillsMet > 0)
            result.Strengths.Add($"Possui {match.DesiredSkillsMet} das {match.DesiredSkillsTotal} skills desejáveis");

        // Pontos fracos
        if (match.RequiredSkillsMet < match.RequiredSkillsTotal)
        {
            var missingCount = match.RequiredSkillsTotal - match.RequiredSkillsMet;
            result.Weaknesses.Add($"Faltam {missingCount} skills obrigatórias da vaga");
        }

        if (!match.ExperienceMatch)
            result.Weaknesses.Add("Senioridade pode não atender o requisitado pela vaga");

        // Sugestões
        if (result.KeywordAnalysis.Missing.Any())
        {
            var topMissing = string.Join(", ", result.KeywordAnalysis.Missing.Take(3));
            result.Suggestions.Add(new Suggestion
            {
                Title = "Destacar experiência com tecnologias da vaga",
                Description = $"Se possui experiência, adicione projetos específicos onde utilizou: {topMissing}"
            });
        }

        result.Suggestions.Add(new Suggestion
        {
            Title = "Customizar currículo para a vaga",
            Description = $"Destaque experiências relacionadas a '{jobDescription.Title}' e mencione as tecnologias requisitadas no topo das suas competências"
        });
    }

    private string GenerateFormattingFeedback(string rawText, int atsScore)
    {
        var issues = new List<string>();
        var lines = rawText.Split('\n');

        // Detectar problemas específicos
        var tableCharacters = new[] { "│", "┌", "└", "├", "┤", "─", "┬", "┴", "┼" };
        var hasTableChars = tableCharacters.Any(c => rawText.Contains(c));
        var linesWithPipes = lines.Count(line => line.Count(c => c == '|') >= 2);

        if (hasTableChars || linesWithPipes > 3)
            issues.Add("tabelas detectadas");

        var linesWithWideSpacing = lines.Count(line =>
            System.Text.RegularExpressions.Regex.Matches(line, @"\s{5,}").Count >= 2);
        if (linesWithWideSpacing > lines.Length * 0.15)
            issues.Add("múltiplas colunas com espaçamento largo");

        var specialCharsCount = rawText.Count(c => "═║╔╗╚╝╠╣╦╩╬▀▄█▌▐░▒▓■□▪▫".Contains(c));
        if (specialCharsCount > 10)
            issues.Add("caracteres especiais/decorativos");

        // Gerar feedback baseado no score e problemas detectados
        if (atsScore >= 90)
        {
            return "Excelente! Currículo possui formatação ideal para ATS. Estrutura clara e sem elementos que prejudiquem o parsing automático.";
        }

        if (atsScore >= 75)
        {
            var feedback = "Boa formatação ATS";
            if (issues.Any())
            {
                feedback += $", mas foram detectados: {string.Join(", ", issues)}. ";
                feedback += "Considere simplificar esses elementos para melhor compatibilidade.";
            }
            else
            {
                feedback += ". Estrutura adequada com pequeno espaço para melhorias.";
            }
            return feedback;
        }

        if (atsScore >= 60)
        {
            var feedback = "Formatação com problemas moderados para ATS. ";
            if (issues.Any())
            {
                feedback += $"Detectados: {string.Join(", ", issues)}. ";
            }
            feedback += "Recomendações: use estrutura linear simples, evite tabelas e colunas múltiplas, remova elementos gráficos.";
            return feedback;
        }

        var criticalFeedback = "⚠️ ATENÇÃO: Formatação inadequada para sistemas ATS. ";
        if (issues.Any())
        {
            criticalFeedback += $"Problemas críticos: {string.Join(", ", issues)}. ";
        }
        criticalFeedback += "Reformate urgentemente usando: texto linear, seções simples com títulos claros, sem tabelas, sem colunas, sem elementos gráficos. ATS pode não conseguir ler este currículo corretamente.";
        return criticalFeedback;
    }
}
