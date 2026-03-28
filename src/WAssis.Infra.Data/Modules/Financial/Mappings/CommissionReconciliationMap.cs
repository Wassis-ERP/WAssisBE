using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Financial.Entities;

namespace WAssis.Infra.Data.Modules.Financial.Mappings;

public sealed class CommissionReconciliationMap : IEntityTypeConfiguration<CommissionReconciliation>
{
    public void Configure(EntityTypeBuilder<CommissionReconciliation> builder)
    {
        builder.ToTable("commission_reconciliations", "financial");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExpectedAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.ReceivedAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.DifferenceAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasIndex(x => x.CommissionReceiptId);

        builder.HasOne<CommissionReceipt>()
            .WithMany()
            .HasForeignKey(x => x.CommissionReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
