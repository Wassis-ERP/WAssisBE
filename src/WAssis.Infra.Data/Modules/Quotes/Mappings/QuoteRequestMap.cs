using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Infra.Data.Modules.Quotes.Mappings;

public sealed class QuoteRequestMap : IEntityTypeConfiguration<QuoteRequest>
{
    public void Configure(EntityTypeBuilder<QuoteRequest> builder)
    {
        builder.ToTable("quote_requests", "quotes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DocumentNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.VehiclePlate).HasMaxLength(16);
        builder.Property(x => x.VehicleBrand).HasMaxLength(100);
        builder.Property(x => x.VehicleModel).HasMaxLength(100);
        builder.Property(x => x.ShareToken).HasMaxLength(64);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.QuoteRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
