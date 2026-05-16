using WAssis.Application.Modules.Quotes.Dtos;

namespace WAssis.Application.Modules.Quotes.Interfaces;

public interface IQuoteProviderRegistry
{
    Task<IReadOnlyCollection<IQuoteProvider>> GetEnabledProvidersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyCollection<QuoteProviderDescriptorDto>> DescribeProvidersAsync(CancellationToken cancellationToken);
}
