using Microsoft.AspNetCore.Http;

namespace WAssis.Services.Api.Modules.Documents.Contracts;

public sealed class UploadProposalDocumentRequest
{
    public string? CorrelationId { get; init; }
    public string Source { get; init; } = "manual_upload";
    public IFormFile File { get; init; } = default!;
}
