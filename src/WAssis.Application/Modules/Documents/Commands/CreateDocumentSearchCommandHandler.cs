using MediatR;
using WAssis.Application.Modules.Documents.Dtos;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;

namespace WAssis.Application.Modules.Documents.Commands;

public sealed class CreateDocumentSearchCommandHandler(IDocumentSearchRepository repository)
    : IRequestHandler<CreateDocumentSearchCommand, DocumentSearchDto>
{
    public async Task<DocumentSearchDto> Handle(CreateDocumentSearchCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        if (existing is not null)
        {
            return new DocumentSearchDto(
                existing.Id,
                existing.CorrelationId,
                existing.InsuranceCompanyCode,
                existing.SearchType,
                existing.Status,
                existing.CreatedAtUtc);
        }

        var search = DocumentSearch.Create(request.CorrelationId, request.InsuranceCompanyCode, request.SearchType);
        await repository.AddAsync(search, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new DocumentSearchDto(
            search.Id,
            search.CorrelationId,
            search.InsuranceCompanyCode,
            search.SearchType,
            search.Status,
            search.CreatedAtUtc);
    }
}
