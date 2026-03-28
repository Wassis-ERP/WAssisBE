namespace WAssis.Services.Api.Modules.Quotes.ViewModels;

public sealed record QuoteProviderDescriptorViewModel(
    string ProviderCode,
    string ProviderName,
    bool IsEnabled,
    bool IsReady,
    string AuthMode,
    string? ProductLine,
    string? OfficialDocumentationUrl,
    IReadOnlyCollection<QuoteProviderRequirementViewModel> Requirements,
    IReadOnlyCollection<QuoteStatusMessageViewModel> Messages);
