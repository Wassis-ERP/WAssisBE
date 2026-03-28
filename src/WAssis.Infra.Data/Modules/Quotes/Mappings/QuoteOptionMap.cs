using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Quotes.Entities;
using WAssis.Domain.Modules.Quotes.ValueObjects;

namespace WAssis.Infra.Data.Modules.Quotes.Mappings;

public sealed class QuoteOptionMap : IEntityTypeConfiguration<QuoteOption>
{
    public void Configure(EntityTypeBuilder<QuoteOption> builder)
    {
        builder.ToTable("quote_options", "quotes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.QuoteRequestId).IsRequired();
        builder.Property(x => x.InsuranceCompanyCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.InsuranceCompanyName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ProductCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProductName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ExternalReference).HasMaxLength(100);
        builder.Ignore(x => x.Coverages);
        builder.Ignore(x => x.Installments);
        builder.Ignore(x => x.Messages);

        builder.OwnsMany<CoverageSnapshot>("_coverages", coverageBuilder =>
        {
            coverageBuilder.ToTable("quote_option_coverages", "quotes");
            coverageBuilder.WithOwner().HasForeignKey("QuoteOptionId");
            coverageBuilder.Property<Guid>("Id");
            coverageBuilder.HasKey("Id");
            coverageBuilder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            coverageBuilder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        });

        builder.OwnsMany<InstallmentSnapshot>("_installments", installmentBuilder =>
        {
            installmentBuilder.ToTable("quote_option_installments", "quotes");
            installmentBuilder.WithOwner().HasForeignKey("QuoteOptionId");
            installmentBuilder.Property<Guid>("Id");
            installmentBuilder.HasKey("Id");
            installmentBuilder.Property(x => x.Number).IsRequired();
            installmentBuilder.Property(x => x.Amount).HasColumnType("numeric(18,2)").IsRequired();
            installmentBuilder.Property(x => x.TotalAmount).HasColumnType("numeric(18,2)");
        });

        builder.OwnsMany<QuoteStatusMessage>("_messages", messageBuilder =>
        {
            messageBuilder.ToTable("quote_option_messages", "quotes");
            messageBuilder.WithOwner().HasForeignKey("QuoteOptionId");
            messageBuilder.Property<Guid>("Id");
            messageBuilder.HasKey("Id");
            messageBuilder.Property(x => x.Code).HasMaxLength(50).IsRequired();
            messageBuilder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        });

        builder.Navigation("_coverages").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_installments").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation("_messages").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
