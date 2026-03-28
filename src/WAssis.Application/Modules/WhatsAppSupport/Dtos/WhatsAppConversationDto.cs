using WAssis.Domain.Modules.WhatsAppSupport.Enums;

namespace WAssis.Application.Modules.WhatsAppSupport.Dtos;

public sealed record WhatsAppConversationDto(
    Guid Id,
    string CorrelationId,
    string CustomerIdentifier,
    string LastMessagePreview,
    WhatsAppConversationStatus Status,
    WhatsAppConversationPriority Priority,
    string? AssignedToUserId,
    string? AssignedToDisplayName,
    DateTime? SlaDueAtUtc,
    DateTime CreatedAtUtc,
    DateTime? StartedHumanAtUtc,
    DateTime? ClosedAtUtc,
    DateTime? UpdatedAtUtc);
