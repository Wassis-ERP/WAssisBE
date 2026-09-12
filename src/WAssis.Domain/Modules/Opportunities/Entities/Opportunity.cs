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
    public Guid? OriginPolicyId { get; private set; }
    public string? LeadName { get; private set; }
    public string? LeadDocumentNumber { get; private set; }
    public string? LeadEmail { get; private set; }
    public string? LeadPhoneNumber { get; private set; }
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public string? Priority { get; private set; }
    public decimal? EstimatedPremiumAmount { get; private set; }
    public decimal? EstimatedCommissionAmount { get; private set; }
    public decimal? EstimatedCommissionPercentage { get; private set; }
    public DateOnly? OpenedOn { get; private set; }
    public DateOnly? ExpectedCloseDate { get; private set; }
    public DateTime? WonAtUtc { get; private set; }
    public DateTime? LostAtUtc { get; private set; }
    public string? LossReasonNotes { get; private set; }
    public string? Campaign { get; private set; }
    public string? InternalNotes { get; private set; }
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
        string? metadataJson,
        Guid? originPolicyId,
        string? leadName,
        string? leadDocumentNumber,
        string? leadEmail,
        string? leadPhoneNumber,
        string? title,
        string? description,
        string? priority,
        decimal? estimatedPremiumAmount,
        decimal? estimatedCommissionAmount,
        decimal? estimatedCommissionPercentage,
        DateOnly? openedOn,
        DateOnly? expectedCloseDate,
        DateTime? wonAtUtc,
        DateTime? lostAtUtc,
        string? lossReasonNotes,
        string? campaign,
        string? internalNotes)
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
            metadataJson,
            originPolicyId,
            leadName,
            leadDocumentNumber,
            leadEmail,
            leadPhoneNumber,
            title,
            description,
            priority,
            estimatedPremiumAmount,
            estimatedCommissionAmount,
            estimatedCommissionPercentage,
            openedOn,
            expectedCloseDate,
            wonAtUtc,
            lostAtUtc,
            lossReasonNotes,
            campaign,
            internalNotes);
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
        string? metadataJson,
        Guid? originPolicyId = null,
        string? leadName = null,
        string? leadDocumentNumber = null,
        string? leadEmail = null,
        string? leadPhoneNumber = null,
        string? title = null,
        string? description = null,
        string? priority = null,
        decimal? estimatedPremiumAmount = null,
        decimal? estimatedCommissionAmount = null,
        decimal? estimatedCommissionPercentage = null,
        DateOnly? openedOn = null,
        DateOnly? expectedCloseDate = null,
        DateTime? wonAtUtc = null,
        DateTime? lostAtUtc = null,
        string? lossReasonNotes = null,
        string? campaign = null,
        string? internalNotes = null)
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
            metadataJson,
            originPolicyId,
            leadName,
            leadDocumentNumber,
            leadEmail,
            leadPhoneNumber,
            title,
            description,
            priority,
            estimatedPremiumAmount,
            estimatedCommissionAmount,
            estimatedCommissionPercentage,
            openedOn,
            expectedCloseDate,
            wonAtUtc,
            lostAtUtc,
            lossReasonNotes,
            campaign,
            internalNotes);
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
        string? metadataJson,
        Guid? originPolicyId = null,
        string? leadName = null,
        string? leadDocumentNumber = null,
        string? leadEmail = null,
        string? leadPhoneNumber = null,
        string? title = null,
        string? description = null,
        string? priority = null,
        decimal? estimatedPremiumAmount = null,
        decimal? estimatedCommissionAmount = null,
        decimal? estimatedCommissionPercentage = null,
        DateOnly? openedOn = null,
        DateOnly? expectedCloseDate = null,
        DateTime? wonAtUtc = null,
        DateTime? lostAtUtc = null,
        string? lossReasonNotes = null,
        string? campaign = null,
        string? internalNotes = null)
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
        Status = string.IsNullOrWhiteSpace(status) ? "pending" : status.Trim().ToLowerInvariant();
        BusinessType = Normalize(businessType);
        ContactType = contactType;
        NetPremium = netPremium;
        CommissionPercentage = commissionPercentage;
        AgencyPercentage = agencyPercentage;
        ProductionAmount = productionAmount;
        ValidityStartUtc = validityStartUtc;
        ValidityEndUtc = validityEndUtc;
        NextFollowUpUtc = nextFollowUpUtc;
        Referrer = Normalize(referrer);
        Notes = Normalize(notes);
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? "{}" : metadataJson;
        OriginPolicyId = originPolicyId;
        LeadName = insuredPersonId.HasValue ? null : Normalize(leadName) ?? Name;
        LeadDocumentNumber = insuredPersonId.HasValue ? null : Normalize(leadDocumentNumber);
        LeadEmail = insuredPersonId.HasValue ? null : Normalize(leadEmail);
        LeadPhoneNumber = insuredPersonId.HasValue ? null : Normalize(leadPhoneNumber);
        Title = Normalize(title) ?? Name;
        Description = Normalize(description) ?? Notes;
        Priority = Normalize(priority);
        EstimatedPremiumAmount = estimatedPremiumAmount ?? netPremium;
        EstimatedCommissionAmount = estimatedCommissionAmount;
        EstimatedCommissionPercentage = estimatedCommissionPercentage ?? commissionPercentage;
        OpenedOn = openedOn ?? DateOnly.FromDateTime(CreatedAtUtc);
        ExpectedCloseDate = expectedCloseDate;
        if (Status == "won")
        {
            WonAtUtc = wonAtUtc ?? concludedAtUtc;
            LostAtUtc = null;
            ConcludedAtUtc = concludedAtUtc ?? WonAtUtc;
        }
        else if (Status == "lost")
        {
            WonAtUtc = null;
            LostAtUtc = lostAtUtc ?? concludedAtUtc;
            ConcludedAtUtc = concludedAtUtc ?? LostAtUtc;
        }
        else
        {
            WonAtUtc = null;
            LostAtUtc = null;
            ConcludedAtUtc = null;
        }
        LossReasonNotes = Normalize(lossReasonNotes);
        Campaign = Normalize(campaign);
        InternalNotes = Normalize(internalNotes);
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
