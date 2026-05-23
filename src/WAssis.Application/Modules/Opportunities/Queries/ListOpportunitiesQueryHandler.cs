using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Interfaces;

namespace WAssis.Application.Modules.Opportunities.Queries;

public sealed class ListOpportunitiesQueryHandler(IOpportunityRepository repository)
    : IRequestHandler<ListOpportunitiesQuery, IReadOnlyCollection<OpportunityDto>>
{
    public async Task<IReadOnlyCollection<OpportunityDto>> Handle(ListOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var opportunities = await repository.ListAsync(request.PipelineId, request.StageId, request.Status, cancellationToken);
        return opportunities.Select(OpportunityMappings.ToDto).ToArray();
    }
}
