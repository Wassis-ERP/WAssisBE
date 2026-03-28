using MediatR;
using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed record ReviewImportedDocumentCommand(
    Guid ImportedDocumentId,
    string? Notes) : IRequest<ImportedDocumentDto?>;
