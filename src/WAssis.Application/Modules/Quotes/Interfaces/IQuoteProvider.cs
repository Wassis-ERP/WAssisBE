using WAssis.Application.Modules.Quotes.Dtos;

namespace WAssis.Application.Modules.Quotes.Interfaces;

public interface IQuoteProvider
{
    string ProviderCode { get; }
    string ProviderName { get; }
    bool IsEnabled { get; }
    QuoteProviderDescriptorDto Describe();
    Task<IReadOnlyCollection<QuoteProviderResultDto>> StartQuoteAsync(
        StartQuoteProcessingDto request,
        CancellationToken cancellationToken);
}
