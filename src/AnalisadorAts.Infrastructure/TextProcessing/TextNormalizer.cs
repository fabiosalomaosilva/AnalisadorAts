using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AnalisadorAts.Infrastructure.TextProcessing;

public static class TextNormalizer
{
    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Converter para minúsculas
        text = text.ToLowerInvariant();

        // Remover acentos
        text = RemoveAccents(text);

        // Remover caracteres especiais, mantendo apenas letras, números e espaços
        text = Regex.Replace(text, @"[^a-z0-9\s@.\-+#()]", " ");

        // Normalizar espaços múltiplos
        text = Regex.Replace(text, @"\s+", " ");

        return text.Trim();
    }

    public static string RemoveAccents(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
