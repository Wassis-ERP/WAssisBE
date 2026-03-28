namespace WAssis.Application.Modules.Documents.Interfaces;

public interface IPdfTextExtractor
{
    Task<string> ExtractTextAsync(byte[] content, CancellationToken cancellationToken);
}
