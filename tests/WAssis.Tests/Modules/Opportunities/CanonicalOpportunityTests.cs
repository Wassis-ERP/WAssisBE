using System.Text.Json;
using WAssis.Application.Modules.Opportunities.Commands;
using WAssis.Application.Modules.Opportunities.Validators;
using WAssis.Domain.Modules.Opportunities.Entities;

namespace WAssis.Tests.Modules.Opportunities;

public sealed class CanonicalOpportunityTests
{
    [Fact]
    public void LegacyMetadataCannotCarryBusinessData()
    {
        var request = JsonSerializer.Deserialize<CreateOpportunityCommand>("""
            {"OfficeBranchId":"44444444-4444-4444-4444-444444444444","Name":"Lead",
             "StageId":"66666666-6666-6666-6666-666666666666","MetadataJson":"{}"}
            """)!;
        var validator = new CreateOpportunityCommandValidator();
        Assert.True(validator.Validate(request).IsValid);
        Assert.False(validator.Validate(request with { MetadataJson = "{\"risk\":100}" }).IsValid);
        Assert.False(validator.Validate(request with { BusinessType = "legacy" }).IsValid);
        Assert.False(validator.Validate(request with { ValidityStartUtc = DateTime.UtcNow }).IsValid);
    }

    [Fact]
    public void CompatibilityAliasesAreDerivedFromCanonicalFields()
    {
        var opportunity = Opportunity.Create("tenant", "branch", "Alias", "responsible", null,
            null, null, null, null, null, "won", null, null, 100, 15, null,
            null, null, null, null, null, null,
            title: "Título canônico", estimatedPremiumAmount: 200, estimatedCommissionPercentage: 20,
            wonAtUtc: new DateTime(2026, 10, 12, 0, 0, 0, DateTimeKind.Utc));
        Assert.Equal(opportunity.Title, opportunity.Name);
        Assert.Equal(opportunity.EstimatedPremiumAmount, opportunity.NetPremium);
        Assert.Equal(opportunity.EstimatedCommissionPercentage, opportunity.CommissionPercentage);
        Assert.Equal(opportunity.WonAtUtc, opportunity.ConcludedAtUtc);
        Assert.Equal("won", opportunity.Status);
        Assert.Equal("{}", opportunity.MetadataJson);
    }
}
