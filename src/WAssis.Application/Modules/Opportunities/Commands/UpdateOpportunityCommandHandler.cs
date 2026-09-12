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
            request.MetadataJson,
            request.OriginPolicyId ?? opportunity.OriginPolicyId,
            request.LeadName ?? opportunity.LeadName,
            request.LeadDocumentNumber ?? opportunity.LeadDocumentNumber,
            request.LeadEmail ?? opportunity.LeadEmail,
            request.LeadPhoneNumber ?? opportunity.LeadPhoneNumber,
            request.Title ?? request.Name ?? opportunity.Title,
            request.Description ?? request.Notes ?? opportunity.Description,
            request.Priority ?? opportunity.Priority,
            request.EstimatedPremiumAmount ?? request.NetPremium ?? opportunity.EstimatedPremiumAmount,
            request.EstimatedCommissionAmount ?? opportunity.EstimatedCommissionAmount,
            request.EstimatedCommissionPercentage ?? request.CommissionPercentage ?? opportunity.EstimatedCommissionPercentage,
            request.OpenedOn ?? opportunity.OpenedOn,
            request.ExpectedCloseDate ?? opportunity.ExpectedCloseDate,
            request.WonAtUtc ?? opportunity.WonAtUtc,
            request.LostAtUtc ?? opportunity.LostAtUtc,
            request.LossReasonNotes ?? opportunity.LossReasonNotes,
            request.Campaign ?? opportunity.Campaign,
            request.InternalNotes ?? opportunity.InternalNotes);

        await repository.SaveChangesAsync(cancellationToken);

        return OpportunityMappings.ToDto(opportunity);
    }
}
