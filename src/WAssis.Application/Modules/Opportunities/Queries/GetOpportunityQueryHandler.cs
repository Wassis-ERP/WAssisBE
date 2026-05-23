using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Interfaces;

namespace WAssis.Application.Modules.Opportunities.Queries;

public sealed class GetOpportunityQueryHandler(IOpportunityRepository repository)
    : IRequestHandler<GetOpportunityQuery, OpportunityDto?>
{
    public async Task<OpportunityDto?> Handle(GetOpportunityQuery request, CancellationToken cancellationToken)
    {
        var opportunity = await repository.GetByIdAsync(request.Id, cancellationToken);
        return opportunity is null ? null : OpportunityMappings.ToDto(opportunity);
    }
}
