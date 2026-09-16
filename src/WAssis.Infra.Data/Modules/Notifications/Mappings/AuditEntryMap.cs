using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Core.Auditing;

namespace WAssis.Infra.Data.Modules.Notifications.Mappings;

public sealed class AuditEntryMap : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("audit_entries", "operations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OfficeBranchId).HasColumnName("office_branch_id").HasMaxLength(64);
        builder.HasIndex(x => new { x.TenantId, x.OfficeBranchId });
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Module).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
