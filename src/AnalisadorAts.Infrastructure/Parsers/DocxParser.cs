using AnalisadorAts.Core.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace AnalisadorAts.Infrastructure.Parsers;

public class DocxParser : IDocumentParser
{
    public bool CanParse(string fileName)
    {
        return fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> ExtractTextAsync(Stream fileStream, string fileName)
    {
        return await Task.Run(() =>
        {
            var text = new StringBuilder();

            using (var document = WordprocessingDocument.Open(fileStream, false))
            {
                var body = document.MainDocumentPart?.Document?.Body;
                if (body != null)
                {
                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        text.AppendLine(paragraph.InnerText);
                    }
                }
            }

            return text.ToString();
        });
    }
}
