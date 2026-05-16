using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Quotes.Entities;

namespace WAssis.Infra.Data.Modules.Quotes.Mappings;

public sealed class QuoteProviderActivationSettingMap : IEntityTypeConfiguration<QuoteProviderActivationSetting>
{
    public void Configure(EntityTypeBuilder<QuoteProviderActivationSetting> builder)
    {
        builder.ToTable("quote_provider_activation_settings", "quotes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();

        builder.HasIndex(x => x.ProviderCode).IsUnique();
    }
}
