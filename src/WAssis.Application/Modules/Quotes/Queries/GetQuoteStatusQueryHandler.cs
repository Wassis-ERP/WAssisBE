using MediatR;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;

namespace WAssis.Application.Modules.Quotes.Queries;

public sealed class GetQuoteStatusQueryHandler(IQuoteRequestRepository repository)
    : IRequestHandler<GetQuoteStatusQuery, QuoteRequestDto?>
{
    public async Task<QuoteRequestDto?> Handle(GetQuoteStatusQuery request, CancellationToken cancellationToken)
    {
        var quoteRequest = await repository.GetByIdAsync(request.QuoteRequestId, cancellationToken);
        return quoteRequest is null ? null : QuoteRequestMappings.ToDto(quoteRequest);
    }
}
