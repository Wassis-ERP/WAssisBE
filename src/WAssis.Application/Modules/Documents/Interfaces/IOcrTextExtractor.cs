namespace WAssis.Application.Modules.Documents.Interfaces;

public interface IOcrTextExtractor
{
    Task<string?> ExtractTextAsync(byte[] content, CancellationToken cancellationToken);
}
