using MediatR;
using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed record ReprocessImportedDocumentCommand(
    Guid ImportedDocumentId) : IRequest<ImportedDocumentDto?>;
