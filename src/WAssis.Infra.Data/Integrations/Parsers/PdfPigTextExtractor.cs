using System.Text;
using UglyToad.PdfPig;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.Infra.Data.Integrations.Parsers;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    private const int MaxPages = 50;
    private const int MaxExtractedCharacters = 1_000_000;

    public Task<string> ExtractTextAsync(byte[] content, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(content, writable: false);
        using var document = PdfDocument.Open(stream);
        if (document.NumberOfPages > MaxPages)
            throw new InvalidDataException("document_page_limit_exceeded");
        var builder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            builder.AppendLine(page.Text);
            if (builder.Length > MaxExtractedCharacters)
                throw new InvalidDataException("document_text_limit_exceeded");
        }

        return Task.FromResult(builder.ToString());
    }
}
