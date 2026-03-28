using WAssis.Application.Modules.Policies.Dtos;
using WAssis.Domain.Modules.Policies.Entities;

namespace WAssis.Application.Modules.Policies;

internal static class PolicyDraftMappings
{
    public static PolicyDraftDto ToDto(PolicyDraft draft)
    {
        return new PolicyDraftDto(
            draft.Id,
            draft.ImportedDocumentId,
            draft.CorrelationId,
            draft.InsuranceCompanyName,
            draft.ProposalNumber,
            draft.InsuredName,
            draft.CoverageStartDateUtc,
            draft.CoverageEndDateUtc,
            draft.TotalPremiumAmount,
            draft.CommissionAmount,
            draft.Status,
            draft.PolicyNumber,
            draft.Notes,
            draft.CreatedAtUtc,
            draft.ReviewedAtUtc,
            draft.ReviewedByUserId,
            draft.ReadyForIssuanceAtUtc,
            draft.IssuedAtUtc);
    }
}
