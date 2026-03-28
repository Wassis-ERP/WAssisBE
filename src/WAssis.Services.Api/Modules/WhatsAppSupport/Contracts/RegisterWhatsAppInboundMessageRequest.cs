namespace WAssis.Services.Api.Modules.WhatsAppSupport.Contracts;

public sealed record RegisterWhatsAppInboundMessageRequest(
    string? CorrelationId,
    string CustomerIdentifier,
    string MessagePreview,
    bool RequestHumanHandoff);
