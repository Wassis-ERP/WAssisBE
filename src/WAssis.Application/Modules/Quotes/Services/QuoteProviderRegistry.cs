using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;

namespace WAssis.Application.Modules.Quotes.Services;

public sealed class QuoteProviderRegistry(IEnumerable<IQuoteProvider> providers) : IQuoteProviderRegistry
{
    public IReadOnlyCollection<IQuoteProvider> GetEnabledProviders()
    {
        return providers.Where(static provider => provider.IsEnabled).ToArray();
    }

    public IReadOnlyCollection<QuoteProviderDescriptorDto> DescribeProviders()
    {
        return providers
            .Select(static provider => provider.Describe())
            .OrderBy(static provider => provider.ProviderName)
            .ToArray();
    }
}
