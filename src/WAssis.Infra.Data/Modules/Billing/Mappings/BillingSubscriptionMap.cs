using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Billing.Entities;

namespace WAssis.Infra.Data.Modules.Billing.Mappings;

public sealed class BillingSubscriptionMap : IEntityTypeConfiguration<BillingSubscription>
{
    public void Configure(EntityTypeBuilder<BillingSubscription> builder)
    {
        builder.ToTable("billing_subscriptions", "billing");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CustomerDisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PlanCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.PlanName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Amount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.BillingDayOfMonth).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Recurrence).IsRequired();
        builder.Property(x => x.StartsAtUtc).IsRequired();
        builder.Property(x => x.NextInvoiceDueDateUtc).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.CancellationReason).HasMaxLength(1000);
    }
}
