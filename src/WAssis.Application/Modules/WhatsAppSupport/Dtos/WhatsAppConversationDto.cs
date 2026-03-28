using WAssis.Domain.Modules.WhatsAppSupport.Enums;

namespace WAssis.Application.Modules.WhatsAppSupport.Dtos;

public sealed record WhatsAppConversationDto(
    Guid Id,
    string CorrelationId,
    string CustomerIdentifier,
    string LastMessagePreview,
    WhatsAppConversationStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
