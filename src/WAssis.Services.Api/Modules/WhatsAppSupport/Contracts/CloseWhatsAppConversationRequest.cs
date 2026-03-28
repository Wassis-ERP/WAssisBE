namespace WAssis.Services.Api.Modules.WhatsAppSupport.Contracts;

public sealed record CloseWhatsAppConversationRequest(
    string LastMessagePreview);
