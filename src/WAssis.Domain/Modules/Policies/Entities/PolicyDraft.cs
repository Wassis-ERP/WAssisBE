using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.Policies.Enums;

namespace WAssis.Domain.Modules.Policies.Entities;

public class PolicyDraft : AggregateRoot
{
    public Guid ImportedDocumentId { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public string? InsuranceCompanyName { get; private set; }
    public string? ProposalNumber { get; private set; }
    public string? InsuredName { get; private set; }
    public DateTime? CoverageStartDateUtc { get; private set; }
    public DateTime? CoverageEndDateUtc { get; private set; }
    public decimal? TotalPremiumAmount { get; private set; }
    public decimal? CommissionAmount { get; private set; }
    public PolicyDraftStatus Status { get; private set; }
    public string? PolicyNumber { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReadyForIssuanceAtUtc { get; private set; }
    public DateTime? IssuedAtUtc { get; private set; }

    private PolicyDraft()
    {
    }

    private PolicyDraft(
        Guid id,
        Guid importedDocumentId,
        string correlationId,
        string? insuranceCompanyName,
        string? proposalNumber,
        string? insuredName,
        DateTime? coverageStartDateUtc,
        DateTime? coverageEndDateUtc,
        decimal? totalPremiumAmount,
        decimal? commissionAmount,
        string? notes)
    {
        Id = id;
        ImportedDocumentId = importedDocumentId;
        CorrelationId = correlationId;
        InsuranceCompanyName = insuranceCompanyName;
        ProposalNumber = proposalNumber;
        InsuredName = insuredName;
        CoverageStartDateUtc = coverageStartDateUtc;
        CoverageEndDateUtc = coverageEndDateUtc;
        TotalPremiumAmount = totalPremiumAmount;
        CommissionAmount = commissionAmount;
        Notes = notes;
        Status = string.IsNullOrWhiteSpace(proposalNumber) || string.IsNullOrWhiteSpace(insuredName)
            ? PolicyDraftStatus.NeedsReview
            : PolicyDraftStatus.Draft;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public static PolicyDraft Create(
        Guid importedDocumentId,
        string correlationId,
        string? insuranceCompanyName,
        string? proposalNumber,
        string? insuredName,
        DateTime? coverageStartDateUtc,
        DateTime? coverageEndDateUtc,
        decimal? totalPremiumAmount,
        decimal? commissionAmount,
        string? notes)
    {
        return new PolicyDraft(
            Guid.NewGuid(),
            importedDocumentId,
            correlationId,
            insuranceCompanyName,
            proposalNumber,
            insuredName,
            coverageStartDateUtc,
            coverageEndDateUtc,
            totalPremiumAmount,
            commissionAmount,
            notes);
    }

    public void MarkReadyForIssuance(string? notes = null)
    {
        Status = PolicyDraftStatus.ReadyForIssuance;
        Notes = notes ?? Notes;
        ReadyForIssuanceAtUtc = DateTime.UtcNow;
    }

    public void MarkIssued(string policyNumber, string? notes = null)
    {
        Status = PolicyDraftStatus.Issued;
        PolicyNumber = policyNumber;
        Notes = notes ?? Notes;
        IssuedAtUtc = DateTime.UtcNow;
    }
}
