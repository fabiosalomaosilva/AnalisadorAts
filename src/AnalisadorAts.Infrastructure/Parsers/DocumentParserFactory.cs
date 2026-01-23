using AnalisadorAts.Core.Interfaces;

namespace AnalisadorAts.Infrastructure.Parsers;

public class DocumentParserFactory
{
    private readonly IEnumerable<IDocumentParser> _parsers;

    public DocumentParserFactory(IEnumerable<IDocumentParser> parsers)
    {
        _parsers = parsers;
    }

    public IDocumentParser GetParser(string fileName)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(fileName));

        if (parser == null)
        {
            throw new NotSupportedException($"Formato de arquivo não suportado: {fileName}");
        }

        return parser;
    }
}
