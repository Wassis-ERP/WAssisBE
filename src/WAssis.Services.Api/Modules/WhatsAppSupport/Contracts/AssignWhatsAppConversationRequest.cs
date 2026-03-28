namespace WAssis.Services.Api.Modules.WhatsAppSupport.Contracts;

public sealed record AssignWhatsAppConversationRequest(
    string? AssignedToUserId,
    string? AssignedToDisplayName);
