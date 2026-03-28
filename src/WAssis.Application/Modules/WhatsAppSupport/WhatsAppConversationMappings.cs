using WAssis.Application.Modules.WhatsAppSupport.Dtos;
using WAssis.Domain.Modules.WhatsAppSupport.Entities;

namespace WAssis.Application.Modules.WhatsAppSupport;

internal static class WhatsAppConversationMappings
{
    public static WhatsAppConversationDto ToDto(WhatsAppConversation conversation)
    {
        return new WhatsAppConversationDto(
            conversation.Id,
            conversation.CorrelationId,
            conversation.CustomerIdentifier,
            conversation.LastMessagePreview,
            conversation.Status,
            conversation.Priority,
            conversation.AssignedToUserId,
            conversation.AssignedToDisplayName,
            conversation.SlaDueAtUtc,
            conversation.CreatedAtUtc,
            conversation.StartedHumanAtUtc,
            conversation.ClosedAtUtc,
            conversation.UpdatedAtUtc);
    }
}
