using MediatR;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;

namespace WAssis.Application.Modules.Quotes.Queries;

public sealed class GetQuoteProvidersQueryHandler(IQuoteProviderRegistry providerRegistry)
    : IRequestHandler<GetQuoteProvidersQuery, IReadOnlyCollection<QuoteProviderDescriptorDto>>
{
    public Task<IReadOnlyCollection<QuoteProviderDescriptorDto>> Handle(
        GetQuoteProvidersQuery request,
        CancellationToken cancellationToken)
    {
        return providerRegistry.DescribeProvidersAsync(cancellationToken);
    }
}
