using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.Infra.Data.Integrations.Parsers;

public sealed class NoOpOcrTextExtractor : IOcrTextExtractor
{
    public Task<string?> ExtractTextAsync(byte[] content, CancellationToken cancellationToken)
    {
        return Task.FromResult<string?>(null);
    }
}
