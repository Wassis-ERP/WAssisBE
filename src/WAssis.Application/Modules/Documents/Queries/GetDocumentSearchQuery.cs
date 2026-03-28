using MediatR;
using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Queries;

public sealed record GetDocumentSearchQuery(Guid DocumentSearchId) : IRequest<DocumentSearchDto?>;
