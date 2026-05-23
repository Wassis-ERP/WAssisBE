using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Application.Modules.Opportunities.Interfaces;
using WAssis.Domain.Modules.Opportunities.Entities;

namespace WAssis.Application.Modules.Opportunities.Commands;

public sealed class CreateOpportunityCommandHandler(
    IOpportunityRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<CreateOpportunityCommand, OpportunityDto>
{
    public async Task<OpportunityDto> Handle(CreateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = Opportunity.Create(
            currentUserContext.ResolveTenantIdOrPlatform(),
            currentUserContext.ResolveBranchIdForWrite(request.OfficeBranchId),
            request.Name,
            request.ResponsibleId ?? currentUserContext.UserId ?? string.Empty,
            request.InsuredPersonId,
            request.PipelineId,
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
            request.MetadataJson);

        await repository.AddAsync(opportunity, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return OpportunityMappings.ToDto(opportunity);
    }
}
