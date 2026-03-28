using WAssis.Application.Modules.Quotes.Dtos;

namespace WAssis.Application.Modules.Quotes.Interfaces;

public interface IQuoteProviderRegistry
{
    IReadOnlyCollection<IQuoteProvider> GetEnabledProviders();
    IReadOnlyCollection<QuoteProviderDescriptorDto> DescribeProviders();
}
