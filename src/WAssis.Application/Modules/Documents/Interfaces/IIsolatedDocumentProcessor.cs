using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Interfaces;

public sealed record IsolatedDocumentProcessingResult(
    ProposalDocumentParsingResultDto? Parsed,
    string? ParsingOrigin,
    string? ErrorCode)
{
    public bool Succeeded => Parsed is not null && ErrorCode is null;
}

public interface IIsolatedDocumentProcessor
{
    Task<IsolatedDocumentProcessingResult> ProcessAsync(string storageKey, CancellationToken cancellationToken);
}
