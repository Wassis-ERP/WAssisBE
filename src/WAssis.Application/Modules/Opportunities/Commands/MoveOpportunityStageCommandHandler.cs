using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Interfaces;

namespace WAssis.Application.Modules.Opportunities.Commands;

public sealed class MoveOpportunityStageCommandHandler(IOpportunityRepository repository)
    : IRequestHandler<MoveOpportunityStageCommand, OpportunityDto?>
{
    public async Task<OpportunityDto?> Handle(MoveOpportunityStageCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await repository.GetForUpdateAsync(request.Id, cancellationToken);
        if (opportunity is null)
        {
            return null;
        }

        opportunity.MoveToStage(request.StageId);
        await repository.SaveChangesAsync(cancellationToken);

        return OpportunityMappings.ToDto(opportunity);
    }
}
