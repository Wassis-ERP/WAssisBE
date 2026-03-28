using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Policies.Entities;

namespace WAssis.Infra.Data.Modules.Policies.Mappings;

public sealed class PolicyDraftMap : IEntityTypeConfiguration<PolicyDraft>
{
    public void Configure(EntityTypeBuilder<PolicyDraft> builder)
    {
        builder.ToTable("policy_drafts", "policies");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.InsuranceCompanyName).HasMaxLength(120);
        builder.Property(x => x.ProposalNumber).HasMaxLength(100);
        builder.Property(x => x.InsuredName).HasMaxLength(200);
        builder.Property(x => x.TotalPremiumAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.CommissionAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.PolicyNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ReviewedByUserId).HasMaxLength(128);
        builder.Property(x => x.ReadyForIssuanceAtUtc);
        builder.Property(x => x.IssuedAtUtc);

        builder.HasIndex(x => x.ImportedDocumentId);
        builder.HasIndex(x => new { x.TenantId, x.PolicyNumber }).IsUnique();
    }
}
