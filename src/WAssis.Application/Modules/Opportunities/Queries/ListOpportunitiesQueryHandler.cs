using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.Opportunities.Queries;

public sealed class ListOpportunitiesQueryHandler(IOpportunityReadRepository repository)
    : IRequestHandler<ListOpportunitiesQuery, PagedResult<OpportunityDto>>
{
    public async Task<PagedResult<OpportunityDto>> Handle(ListOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var opportunities = await repository.ListAsync(request.PipelineId, request.StageId, request.Status, page, pageSize, cancellationToken);
        return new PagedResult<OpportunityDto>(opportunities.Items.Select(OpportunityMappings.ToDto).ToArray(), page, pageSize, opportunities.TotalCount);
    }
}
