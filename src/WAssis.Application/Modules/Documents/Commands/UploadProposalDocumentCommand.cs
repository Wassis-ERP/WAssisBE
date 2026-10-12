using MediatR;
using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed record UploadProposalDocumentCommand(
    Guid DocumentId,
    string CorrelationId,
    string FileName,
    string ContentType,
    string Source,
    string StorageKey) : IRequest<ImportedDocumentDto>;
