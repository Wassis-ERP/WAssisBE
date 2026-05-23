using WAssis.Domain.Core.Entities;

namespace WAssis.Domain.Modules.Opportunities.Entities;

public class Opportunity : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string? OfficeBranchId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ResponsibleId { get; private set; } = string.Empty;
    public Guid? InsuredPersonId { get; private set; }
    public string? PipelineId { get; private set; }
    public string? StageId { get; private set; }
    public string? InsuranceLineId { get; private set; }
    public string? InsurerId { get; private set; }
    public string? OriginId { get; private set; }
    public string? LossReasonId { get; private set; }
    public string Status { get; private set; } = "pending";
    public string? BusinessType { get; private set; }
    public bool? ContactType { get; private set; }
    public decimal? NetPremium { get; private set; }
    public decimal? CommissionPercentage { get; private set; }
    public decimal? AgencyPercentage { get; private set; }
    public decimal? ProductionAmount { get; private set; }
    public DateTime? ValidityStartUtc { get; private set; }
    public DateTime? ValidityEndUtc { get; private set; }
    public DateTime? NextFollowUpUtc { get; private set; }
    public DateTime? ConcludedAtUtc { get; private set; }
    public string? Referrer { get; private set; }
    public string? Notes { get; private set; }
    public string MetadataJson { get; private set; } = "{}";
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Opportunity()
    {
    }

    private Opportunity(
        Guid id,
        string tenantId,
        string? officeBranchId,
        string name,
        string responsibleId,
        Guid? insuredPersonId,
        string? pipelineId,
        string? stageId,
        string? insuranceLineId,
        string? insurerId,
        string? originId,
        string? status,
        string? businessType,
        bool? contactType,
        decimal? netPremium,
        decimal? commissionPercentage,
        decimal? agencyPercentage,
        DateTime? validityStartUtc,
        DateTime? validityEndUtc,
        DateTime? nextFollowUpUtc,
        string? referrer,
        string? notes,
        string? metadataJson)
    {
        Id = id;
        TenantId = tenantId;
        OfficeBranchId = Normalize(officeBranchId);
        CreatedAtUtc = DateTime.UtcNow;
        Update(
            officeBranchId,
            name,
            responsibleId,
            insuredPersonId,
            pipelineId,
            stageId,
            insuranceLineId,
            insurerId,
            originId,
            null,
            status,
            businessType,
            contactType,
            netPremium,
            commissionPercentage,
            agencyPercentage,
            null,
            validityStartUtc,
            validityEndUtc,
            nextFollowUpUtc,
            null,
            referrer,
            notes,
            metadataJson);
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static Opportunity Create(
        string tenantId,
        string? officeBranchId,
        string name,
        string responsibleId,
        Guid? insuredPersonId,
        string? pipelineId,
        string? stageId,
        string? insuranceLineId,
        string? insurerId,
        string? originId,
        string? status,
        string? businessType,
        bool? contactType,
        decimal? netPremium,
        decimal? commissionPercentage,
        decimal? agencyPercentage,
        DateTime? validityStartUtc,
        DateTime? validityEndUtc,
        DateTime? nextFollowUpUtc,
        string? referrer,
        string? notes,
        string? metadataJson)
    {
        return new Opportunity(
            Guid.NewGuid(),
            tenantId,
            officeBranchId,
            name,
            responsibleId,
            insuredPersonId,
            pipelineId,
            stageId,
            insuranceLineId,
            insurerId,
            originId,
            status,
            businessType,
            contactType,
            netPremium,
            commissionPercentage,
            agencyPercentage,
            validityStartUtc,
            validityEndUtc,
            nextFollowUpUtc,
            referrer,
            notes,
            metadataJson);
    }

    public void Update(
        string? officeBranchId,
        string name,
        string responsibleId,
        Guid? insuredPersonId,
        string? pipelineId,
        string? stageId,
        string? insuranceLineId,
        string? insurerId,
        string? originId,
        string? lossReasonId,
        string? status,
        string? businessType,
        bool? contactType,
        decimal? netPremium,
        decimal? commissionPercentage,
        decimal? agencyPercentage,
        decimal? productionAmount,
        DateTime? validityStartUtc,
        DateTime? validityEndUtc,
        DateTime? nextFollowUpUtc,
        DateTime? concludedAtUtc,
        string? referrer,
        string? notes,
        string? metadataJson)
    {
        OfficeBranchId = Normalize(officeBranchId);
        Name = name.Trim();
        ResponsibleId = string.IsNullOrWhiteSpace(responsibleId) ? ResponsibleId : responsibleId.Trim();
        InsuredPersonId = insuredPersonId;
        PipelineId = Normalize(pipelineId);
        StageId = Normalize(stageId);
        InsuranceLineId = Normalize(insuranceLineId);
        InsurerId = Normalize(insurerId);
        OriginId = Normalize(originId);
        LossReasonId = Normalize(lossReasonId);
        Status = string.IsNullOrWhiteSpace(status) ? "pending" : status.Trim();
        BusinessType = Normalize(businessType);
        ContactType = contactType;
        NetPremium = netPremium;
        CommissionPercentage = commissionPercentage;
        AgencyPercentage = agencyPercentage;
        ProductionAmount = productionAmount;
        ValidityStartUtc = validityStartUtc;
        ValidityEndUtc = validityEndUtc;
        NextFollowUpUtc = nextFollowUpUtc;
        ConcludedAtUtc = concludedAtUtc;
        Referrer = Normalize(referrer);
        Notes = Normalize(notes);
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void MoveToStage(string? stageId)
    {
        StageId = Normalize(stageId);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
