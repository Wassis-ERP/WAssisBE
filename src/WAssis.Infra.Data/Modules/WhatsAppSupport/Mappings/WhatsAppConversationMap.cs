using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;

namespace WAssis.Infra.Data.Modules.WhatsAppSupport.Mappings;

public sealed class WhatsAppConversationMap : IEntityTypeConfiguration<WhatsAppConversation>
{
    public void Configure(EntityTypeBuilder<WhatsAppConversation> builder)
    {
        builder.ToTable("whatsapp_conversations", "whatsapp_support");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.CorrelationId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CustomerIdentifier).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastMessagePreview).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
