using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Billing.Entities;

namespace WAssis.Infra.Data.Modules.Billing.Mappings;

public sealed class BillingInvoiceMap : IEntityTypeConfiguration<BillingInvoice>
{
    public void Configure(EntityTypeBuilder<BillingInvoice> builder)
    {
        builder.ToTable("billing_invoices", "billing");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ReferencePeriod).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.ExternalReference).HasMaxLength(120);
        builder.Property(x => x.PaymentMethod).HasMaxLength(40);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.IssuedAtUtc).IsRequired();
        builder.Property(x => x.DueDateUtc).IsRequired();

        builder.HasIndex(x => new { x.BillingSubscriptionId, x.ReferencePeriod }).IsUnique();
    }
}
