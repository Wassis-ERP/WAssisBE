using WAssis.Domain.Modules.WhatsAppSupport.Enums;

namespace WAssis.Services.Api.Modules.WhatsAppSupport.ViewModels;

public sealed record WhatsAppConversationViewModel(
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
