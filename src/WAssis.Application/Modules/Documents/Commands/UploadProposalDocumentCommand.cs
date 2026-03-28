using MediatR;
using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed record UploadProposalDocumentCommand(
    string CorrelationId,
    string FileName,
    string ContentType,
    string Source,
    byte[] Content) : IRequest<ImportedDocumentDto>;
