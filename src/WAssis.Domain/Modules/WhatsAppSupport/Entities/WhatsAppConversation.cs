using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Enums;

namespace WAssis.Domain.Modules.WhatsAppSupport.Entities;

public class WhatsAppConversation : AggregateRoot
{
    public string CorrelationId { get; private set; } = string.Empty;
    public string CustomerIdentifier { get; private set; } = string.Empty;
    public string LastMessagePreview { get; private set; } = string.Empty;
    public WhatsAppConversationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private WhatsAppConversation()
    {
    }

    private WhatsAppConversation(Guid id, string correlationId, string customerIdentifier, string lastMessagePreview)
    {
        Id = id;
        CorrelationId = correlationId;
        CustomerIdentifier = customerIdentifier;
        LastMessagePreview = lastMessagePreview;
        Status = WhatsAppConversationStatus.BotActive;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static WhatsAppConversation Create(string correlationId, string customerIdentifier, string lastMessagePreview)
    {
        return new WhatsAppConversation(Guid.NewGuid(), correlationId, customerIdentifier, lastMessagePreview);
    }

    public void RequestHumanHandoff()
    {
        Status = WhatsAppConversationStatus.WaitingHumanHandoff;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
