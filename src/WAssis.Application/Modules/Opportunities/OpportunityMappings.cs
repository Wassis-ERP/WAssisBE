using WAssis.Application.Modules.Opportunities.Dtos;
using WAssis.Domain.Modules.Opportunities.Entities;

namespace WAssis.Application.Modules.Opportunities;

internal static class OpportunityMappings
{
    public static OpportunityDto ToDto(Opportunity opportunity)
    {
        return new OpportunityDto(
            opportunity.Id,
            opportunity.OfficeBranchId,
            opportunity.Name,
            opportunity.ResponsibleId,
            opportunity.InsuredPersonId,
            opportunity.PipelineId,
            opportunity.StageId,
            opportunity.InsuranceLineId,
            opportunity.InsurerId,
            opportunity.OriginId,
            opportunity.LossReasonId,
            opportunity.Status,
            opportunity.BusinessType,
            opportunity.ContactType,
            opportunity.NetPremium,
            opportunity.CommissionPercentage,
            opportunity.AgencyPercentage,
            opportunity.ProductionAmount,
            opportunity.ValidityStartUtc,
            opportunity.ValidityEndUtc,
            opportunity.NextFollowUpUtc,
            opportunity.ConcludedAtUtc,
            opportunity.Referrer,
            opportunity.Notes,
            opportunity.MetadataJson,
            opportunity.CreatedAtUtc,
            opportunity.UpdatedAtUtc);
    }
}
