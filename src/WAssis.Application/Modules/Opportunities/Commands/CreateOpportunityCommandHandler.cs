using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Domain.Modules.Opportunities.Entities;

namespace WAssis.Application.Modules.Opportunities.Commands;

public sealed class CreateOpportunityCommandHandler(
    IOpportunityRepository repository,
    ICurrentUserContext currentUserContext,
    IOpportunityScope scope)
    : IRequestHandler<CreateOpportunityCommand, OpportunityDto>
{
    public async Task<OpportunityDto> Handle(CreateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var stage = await scope.ValidateAsync(request.OfficeBranchId, request.StageId, request.InsuredPersonId, request.Status, cancellationToken);
        var opportunity = Opportunity.Create(
            currentUserContext.ResolveTenantIdOrPlatform(),
            currentUserContext.ResolveBranchIdForWrite(request.OfficeBranchId),
            request.Name,
            request.ResponsibleId ?? currentUserContext.UserId ?? string.Empty,
            request.InsuredPersonId,
            stage.PipelineId,
            request.StageId,
            request.InsuranceLineId,
            request.InsurerId,
            request.OriginId,
            request.Status ?? "pending",
            request.BusinessType,
            request.ContactType,
            request.NetPremium,
            request.CommissionPercentage,
            request.AgencyPercentage,
            request.ValidityStartUtc,
            request.ValidityEndUtc,
            request.NextFollowUpUtc,
            request.Referrer,
            request.Notes,
            request.MetadataJson,
            request.OriginPolicyId,
            request.LeadName,
            request.LeadDocumentNumber,
            request.LeadEmail,
            request.LeadPhoneNumber,
            request.Title,
            request.Description,
            request.Priority,
            request.EstimatedPremiumAmount,
            request.EstimatedCommissionAmount,
            request.EstimatedCommissionPercentage,
            request.OpenedOn,
            request.ExpectedCloseDate,
            request.WonAtUtc,
            request.LostAtUtc,
            request.LossReasonNotes,
            request.Campaign,
            request.InternalNotes);

        await repository.AddAsync(opportunity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return OpportunityMappings.ToDto(opportunity);
    }
}
