namespace WAssis.Services.Api.Modules.Quotes.ViewModels;

public sealed record QuoteProviderRequirementViewModel(
    string Code,
    string Description,
    bool IsConfigured);
