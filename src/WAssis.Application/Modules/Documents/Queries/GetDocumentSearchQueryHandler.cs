using MediatR;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;

namespace WAssis.Application.Modules.Documents.Queries;

public sealed class GetDocumentSearchQueryHandler(IDocumentSearchRepository repository)
    : IRequestHandler<GetDocumentSearchQuery, DocumentSearchDto?>
{
    public async Task<DocumentSearchDto?> Handle(GetDocumentSearchQuery request, CancellationToken cancellationToken)
    {
        var search = await repository.GetByIdAsync(request.DocumentSearchId, cancellationToken);
        return search is null
            ? null
            : new DocumentSearchDto(
                search.Id,
                search.CorrelationId,
                search.InsuranceCompanyCode,
                search.SearchType,
                search.Status,
                search.CreatedAtUtc);
    }
}
