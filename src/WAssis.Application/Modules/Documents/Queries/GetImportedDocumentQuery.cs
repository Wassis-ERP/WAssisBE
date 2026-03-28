using MediatR;
using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Queries;

public sealed record GetImportedDocumentQuery(Guid ImportedDocumentId) : IRequest<ImportedDocumentDto?>;
