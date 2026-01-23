namespace AnalisadorAts.Core.Interfaces;

public interface IDocumentParser
{
    Task<string> ExtractTextAsync(Stream fileStream, string fileName);
    bool CanParse(string fileName);
}
