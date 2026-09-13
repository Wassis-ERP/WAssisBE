using MediatR;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Interfaces;

namespace WAssis.Application.Modules.Opportunities.Commands;

public sealed class MoveOpportunityStageCommandHandler(IOpportunityRepository repository, IOpportunityScope scope)
    : IRequestHandler<MoveOpportunityStageCommand, OpportunityDto?>
{
    public async Task<OpportunityDto?> Handle(MoveOpportunityStageCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await repository.GetForUpdateAsync(request.Id, cancellationToken);
        if (opportunity is null)
        {
            return null;
        }

        var stage = await scope.ValidateAsync(opportunity.OfficeBranchId, request.StageId, opportunity.InsuredPersonId, opportunity.Status, cancellationToken);
        if (stage.PipelineId != opportunity.PipelineId) throw new FluentValidation.ValidationException("Use a edição para trocar o funil.");
        opportunity.MoveToStage(request.StageId);
        await repository.SaveChangesAsync(cancellationToken);

        return OpportunityMappings.ToDto(opportunity);
    }
}
