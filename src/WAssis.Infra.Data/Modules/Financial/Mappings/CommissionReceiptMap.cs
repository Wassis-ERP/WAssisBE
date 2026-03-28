using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Financial.Entities;

namespace WAssis.Infra.Data.Modules.Financial.Mappings;

public sealed class CommissionReceiptMap : IEntityTypeConfiguration<CommissionReceipt>
{
    public void Configure(EntityTypeBuilder<CommissionReceipt> builder)
    {
        builder.ToTable("commission_receipts", "financial");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.InsuranceCompanyCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReceivedAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.SourceType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReceivedAtUtc).IsRequired();
    }
}
