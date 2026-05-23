using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Interfaces;

namespace WAssis.Application.Modules.Opportunities.Commands;

public sealed class UpdateOpportunityCommandHandler(
    IOpportunityRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<UpdateOpportunityCommand, OpportunityDto?>
{
    public async Task<OpportunityDto?> Handle(UpdateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (opportunity is null)
        {
            return null;
        }

        opportunity.Update(
            currentUserContext.ResolveBranchIdForWrite(request.OfficeBranchId),
            request.Name,
            request.ResponsibleId ?? opportunity.ResponsibleId,
            request.InsuredPersonId,
            request.PipelineId,
            request.StageId,
            request.InsuranceLineId,
            request.InsurerId,
            request.OriginId,
            request.LossReasonId,
            request.Status,
            request.BusinessType,
            request.ContactType,
            request.NetPremium,
            request.CommissionPercentage,
            request.AgencyPercentage,
            request.ProductionAmount,
            request.ValidityStartUtc,
            request.ValidityEndUtc,
            request.NextFollowUpUtc,
            request.ConcludedAtUtc,
            request.Referrer,
            request.Notes,
            request.MetadataJson);

        await repository.SaveChangesAsync(cancellationToken);

        return OpportunityMappings.ToDto(opportunity);
    }
}
