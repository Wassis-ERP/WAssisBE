using WAssis.Domain.Core.Entities;
using WAssis.Domain.Modules.WhatsAppSupport.Enums;

namespace WAssis.Domain.Modules.WhatsAppSupport.Entities;

public class WhatsAppConversation : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string CustomerIdentifier { get; private set; } = string.Empty;
    public string LastMessagePreview { get; private set; } = string.Empty;
    public WhatsAppConversationStatus Status { get; private set; }
    public WhatsAppConversationPriority Priority { get; private set; }
    public string? AssignedToUserId { get; private set; }
    public string? AssignedToDisplayName { get; private set; }
    public DateTime? SlaDueAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? StartedHumanAtUtc { get; private set; }
    public DateTime? ClosedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private WhatsAppConversation()
    {
    }

    private WhatsAppConversation(Guid id, string tenantId, string correlationId, string customerIdentifier, string lastMessagePreview, WhatsAppConversationPriority priority)
    {
        Id = id;
        TenantId = tenantId;
        CorrelationId = correlationId;
        CustomerIdentifier = customerIdentifier;
        LastMessagePreview = lastMessagePreview;
        Priority = priority;
        Status = WhatsAppConversationStatus.BotActive;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
        SlaDueAtUtc = CalculateSla(priority);
    }

    public static WhatsAppConversation Create(
        string tenantId,
        string correlationId,
        string customerIdentifier,
        string lastMessagePreview,
        WhatsAppConversationPriority priority)
    {
        return new WhatsAppConversation(Guid.NewGuid(), tenantId, correlationId, customerIdentifier, lastMessagePreview, priority);
    }

    public void RequestHumanHandoff()
    {
        Status = WhatsAppConversationStatus.WaitingHumanHandoff;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Assign(string? assignedToUserId, string? assignedToDisplayName)
    {
        AssignedToUserId = assignedToUserId;
        AssignedToDisplayName = assignedToDisplayName;
        Status = WhatsAppConversationStatus.HumanActive;
        StartedHumanAtUtc ??= DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Close(string lastMessagePreview)
    {
        LastMessagePreview = lastMessagePreview;
        Status = WhatsAppConversationStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static DateTime CalculateSla(WhatsAppConversationPriority priority)
    {
        return priority switch
        {
            WhatsAppConversationPriority.Critical => DateTime.UtcNow.AddMinutes(5),
            WhatsAppConversationPriority.High => DateTime.UtcNow.AddMinutes(15),
            WhatsAppConversationPriority.Normal => DateTime.UtcNow.AddMinutes(30),
            _ => DateTime.UtcNow.AddHours(1)
        };
    }
}
