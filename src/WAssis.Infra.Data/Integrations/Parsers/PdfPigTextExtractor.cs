using System.Text;
using UglyToad.PdfPig;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.Infra.Data.Integrations.Parsers;

public sealed class PdfPigTextExtractor : IPdfTextExtractor
{
    public Task<string> ExtractTextAsync(byte[] content, CancellationToken cancellationToken)
    {
        using var stream = new MemoryStream(content, writable: false);
        using var document = PdfDocument.Open(stream);
        var builder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            builder.AppendLine(page.Text);
        }

        return Task.FromResult(builder.ToString());
    }
}
