using AnalisadorAts.Core.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace AnalisadorAts.Infrastructure.Parsers;

public class PdfParser : IDocumentParser
{
    public bool CanParse(string fileName)
    {
        return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        return await Task.Run(() =>
        {
            using var document = PdfDocument.Open(fileStream);
            var text = string.Empty;

            foreach (Page page in document.GetPages())
            {
                text += page.Text + "\n";
            }

            return text;
        });
    }
}
