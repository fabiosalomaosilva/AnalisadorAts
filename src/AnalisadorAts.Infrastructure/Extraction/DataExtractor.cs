using AnalisadorAts.Core.Models;
using System.Text.RegularExpressions;

namespace AnalisadorAts.Infrastructure.Extraction;

public class DataExtractor
{
    private static readonly Regex EmailRegex = new(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"\(?\d{2}\)?\s?\d{4,5}-?\d{4}", RegexOptions.Compiled);

    private readonly SkillsDictionary _skillsDictionary;

    public DataExtractor(SkillsDictionary skillsDictionary)
    {
        _skillsDictionary = skillsDictionary;
    }

    public ExtractedData Extract(string rawText, string normalizedText)
    {
        var data = new ExtractedData
        {
            Email = ExtractEmail(rawText),
            Phone = ExtractPhone(rawText),
            Name = ExtractName(rawText),
            Skills = ExtractSkills(normalizedText),
        };

        data.EstimatedSeniority = EstimateSeniority(normalizedText, data.Skills.Count);

        return data;
    }

    private string? ExtractEmail(string text)
    {
        var match = EmailRegex.Match(text);
        return match.Success ? match.Value : null;
    }

    private string? ExtractPhone(string text)
    {
        var match = PhoneRegex.Match(text);
        return match.Success ? match.Value : null;
    }

    private string? ExtractName(string text)
    {
        // Tentar extrair nome das primeiras linhas do texto original (não normalizado)
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines.Take(10))
        {
            var trimmedLine = line.Trim();

            // Ignorar linhas muito curtas ou longas
            if (trimmedLine.Length < 5 || trimmedLine.Length > 80)
                continue;

            // Ignorar se tem @, http, www (provavelmente email/link)
            if (trimmedLine.Contains("@") ||
                trimmedLine.Contains("http") ||
                trimmedLine.Contains("www."))
                continue;

            // Ignorar linhas com muitos números (provavelmente telefone ou data)
            if (trimmedLine.Count(char.IsDigit) > 4)
                continue;

            // Nome geralmente está nas primeiras linhas e tem entre 2-5 palavras
            var words = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // Verificar se parece um nome (2-5 palavras, começa com maiúscula)
            if (words.Length >= 2 && words.Length <= 5)
            {
                // Verificar se todas as palavras começam com maiúscula (padrão de nome)
                bool looksLikeName = words.All(w =>
                    w.Length > 0 &&
                    char.IsUpper(w[0]) &&
                    w.All(c => char.IsLetter(c) || c == '.' || c == '\'' || c == '-'));

                if (looksLikeName)
                {
                    return trimmedLine;
                }
            }
        }

        return null;
    }

    private List<string> ExtractSkills(string normalizedText)
    {
        var foundSkills = new HashSet<string>();

        foreach (var skill in _skillsDictionary.GetAllSkills())
        {
            // Usar word boundary para evitar falsos positivos
            // Ex: "java" não deve match em "javascript"
            if (IsSkillPresent(normalizedText, skill))
            {
                foundSkills.Add(skill);
            }
        }

        return foundSkills.ToList();
    }

    private bool IsSkillPresent(string text, string skill)
    {
        // Normalizar skill para comparação
        var normalizedSkill = skill.ToLower().Trim();

        // Para skills compostas (ex: "sql server", ".net core"), verificar presença direta
        if (normalizedSkill.Contains(" ") || normalizedSkill.Contains("."))
        {
            return text.Contains(normalizedSkill);
        }

        // Para skills simples, usar word boundary
        // Adicionar espaços/caracteres ao redor para garantir palavra completa
        var pattern = $" {normalizedSkill} ";
        var textWithBoundaries = $" {text} ";

        // Verificar também com pontuação comum
        return textWithBoundaries.Contains(pattern) ||
               textWithBoundaries.Contains($" {normalizedSkill},") ||
               textWithBoundaries.Contains($" {normalizedSkill}.") ||
               textWithBoundaries.Contains($" {normalizedSkill};") ||
               textWithBoundaries.Contains($"({normalizedSkill}") ||
               textWithBoundaries.Contains($"({normalizedSkill})") ||
               textWithBoundaries.Contains($",{normalizedSkill}") ||
               textWithBoundaries.Contains($"-{normalizedSkill} ") ||
               textWithBoundaries.Contains($" {normalizedSkill}-");
    }

    private string EstimateSeniority(string text, int skillsCount)
    {
        var juniorKeywords = new[] { "junior", "jr", "estagiario", "trainee", "iniciante" };
        var plenoKeywords = new[] { "pleno", "mid", "intermediario" };
        var seniorKeywords = new[] { "senior", "sr", "especialista", "expert", "arquiteto", "lead", "tech lead" };

        var juniorScore = juniorKeywords.Count(k => text.Contains(k));
        var plenoScore = plenoKeywords.Count(k => text.Contains(k));
        var seniorScore = seniorKeywords.Count(k => text.Contains(k));

        // Considerar também a quantidade de skills
        if (seniorScore > 0 || skillsCount >= 15) return "Senior";
        if (plenoScore > 0 || skillsCount >= 8) return "Pleno";
        if (juniorScore > 0 || skillsCount >= 3) return "Júnior";

        return "Não identificado";
    }
}
