using WAssis.Domain.Modules.WhatsAppSupport.Enums;

namespace WAssis.Services.Api.Modules.WhatsAppSupport.ViewModels;

public sealed record WhatsAppConversationViewModel(
    Guid Id,
    string CorrelationId,
    string CustomerIdentifier,
    string LastMessagePreview,
    WhatsAppConversationStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
