namespace WAssis.Services.Api.Modules.Quotes.ViewModels;

public sealed record QuoteResultsViewModel(
    Guid QuoteRequestId,
    string Status,
    IReadOnlyCollection<QuoteOptionViewModel> Results);
