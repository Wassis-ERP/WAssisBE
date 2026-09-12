using WAssis.Application.Modules.Opportunities.Commands;
using WAssis.Application.Modules.Opportunities.Validators;
using WAssis.Domain.Modules.Opportunities.Entities;

namespace WAssis.Tests.Modules.Opportunities;

public sealed class OpportunityV31ContractTests
{
    [Fact]
    public void LegacyCreate_ProjectsValuesIntoCanonicalFieldsWithoutBusinessMetadata()
    {
        var opportunity = CreateOpportunity(
            insuredPersonId: null,
            status: "pending",
            netPremium: 1200m,
            commissionPercentage: 18m,
            notes: "Descrição legada");

        Assert.Equal("Seguro Auto", opportunity.Title);
        Assert.Equal("Seguro Auto", opportunity.LeadName);
        Assert.Equal(1200m, opportunity.EstimatedPremiumAmount);
        Assert.Equal(18m, opportunity.EstimatedCommissionPercentage);
        Assert.Equal("Descrição legada", opportunity.Description);
        Assert.Equal("{}", opportunity.MetadataJson);
    }

    [Fact]
    public void CanonicalCreate_KeepsLeadAndCommercialFieldsSeparate()
    {
        var originPolicyId = Guid.NewGuid();
        var wonAt = new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Utc);
        var opportunity = CreateOpportunity(
            insuredPersonId: null,
            status: "won",
            concludedAtUtc: wonAt,
            originPolicyId: originPolicyId,
            leadName: "Patrícia Souza",
            leadDocumentNumber: "12345678900",
            leadEmail: "patricia@example.test",
            title: "Lead Auto",
            priority: "ALTA",
            estimatedCommissionAmount: 240m,
            expectedCloseDate: new DateOnly(2026, 9, 30),
            wonAtUtc: wonAt,
            campaign: "Auto Setembro");

        Assert.Equal(originPolicyId, opportunity.OriginPolicyId);
        Assert.Equal("Patrícia Souza", opportunity.LeadName);
        Assert.Equal("Lead Auto", opportunity.Title);
        Assert.Equal("ALTA", opportunity.Priority);
        Assert.Equal(240m, opportunity.EstimatedCommissionAmount);
        Assert.Equal(wonAt, opportunity.WonAtUtc);
        Assert.Equal(wonAt, opportunity.ConcludedAtUtc);
        Assert.Null(opportunity.LostAtUtc);
        Assert.Equal("Auto Setembro", opportunity.Campaign);
    }

    [Fact]
    public void StatusTransition_KeepsLegacyAndCanonicalConclusionFieldsConsistent()
    {
        var wonAt = new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Utc);
        var lostAt = wonAt.AddDays(1);
        var opportunity = CreateOpportunity(
            insuredPersonId: null,
            status: "won",
            wonAtUtc: wonAt);

        opportunity.Update(
            "branch-1", "Seguro Auto", "user-1", null, "pipeline-1", "stage-1", null, null, null, null,
            "lost", null, null, null, null, null, null, null, null, null, lostAt, null, null, "{}",
            wonAtUtc: wonAt,
            lostAtUtc: lostAt);

        Assert.Equal("lost", opportunity.Status);
        Assert.Null(opportunity.WonAtUtc);
        Assert.Equal(lostAt, opportunity.LostAtUtc);
        Assert.Equal(lostAt, opportunity.ConcludedAtUtc);
    }

    [Fact]
    public void Validator_RequiresBranchStageAndConsistentConclusion()
    {
        var command = new CreateOpportunityCommand(
            OfficeBranchId: null,
            Name: "Lead",
            ResponsibleId: null,
            InsuredPersonId: null,
            PipelineId: null,
            StageId: null,
            InsuranceLineId: null,
            InsurerId: null,
            OriginId: null,
            Status: "won",
            BusinessType: null,
            ContactType: null,
            NetPremium: null,
            CommissionPercentage: null,
            AgencyPercentage: null,
            ValidityStartUtc: null,
            ValidityEndUtc: null,
            NextFollowUpUtc: null,
            Referrer: null,
            Notes: null,
            MetadataJson: "{}",
            WonAtUtc: null,
            LostAtUtc: DateTime.UtcNow);

        var result = new CreateOpportunityCommandValidator().Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.OfficeBranchId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.StageId));
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Won opportunities", StringComparison.Ordinal));
    }

    private static Opportunity CreateOpportunity(
        Guid? insuredPersonId,
        string status,
        decimal? netPremium = null,
        decimal? commissionPercentage = null,
        string? notes = null,
        DateTime? concludedAtUtc = null,
        Guid? originPolicyId = null,
        string? leadName = null,
        string? leadDocumentNumber = null,
        string? leadEmail = null,
        string? title = null,
        string? priority = null,
        decimal? estimatedCommissionAmount = null,
        DateOnly? expectedCloseDate = null,
        DateTime? wonAtUtc = null,
        string? campaign = null)
    {
        var opportunity = Opportunity.Create(
            tenantId: "tenant-1",
            officeBranchId: "branch-1",
            name: "Seguro Auto",
            responsibleId: "user-1",
            insuredPersonId: insuredPersonId,
            pipelineId: "pipeline-1",
            stageId: "stage-1",
            insuranceLineId: null,
            insurerId: null,
            originId: null,
            status: status,
            businessType: null,
            contactType: null,
            netPremium: netPremium,
            commissionPercentage: commissionPercentage,
            agencyPercentage: null,
            validityStartUtc: null,
            validityEndUtc: null,
            nextFollowUpUtc: null,
            referrer: null,
            notes: notes,
            metadataJson: "{}",
            originPolicyId: originPolicyId,
            leadName: leadName,
            leadDocumentNumber: leadDocumentNumber,
            leadEmail: leadEmail,
            title: title,
            priority: priority,
            estimatedCommissionAmount: estimatedCommissionAmount,
            expectedCloseDate: expectedCloseDate,
            wonAtUtc: wonAtUtc);

        if (concludedAtUtc.HasValue)
        {
            opportunity.Update(
                "branch-1", "Seguro Auto", "user-1", insuredPersonId, "pipeline-1", "stage-1", null, null, null, null,
                status, null, null, netPremium, commissionPercentage, null, null, null, null, null, concludedAtUtc, null, notes, "{}",
                originPolicyId, leadName, leadDocumentNumber, leadEmail, null, title, null, priority, netPremium,
                estimatedCommissionAmount, commissionPercentage, null, expectedCloseDate, wonAtUtc, null, null, campaign, null);
        }

        return opportunity;
    }
}
