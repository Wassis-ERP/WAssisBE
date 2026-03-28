using MediatR;
using WAssis.Application.Modules.Quotes.Dtos;

namespace WAssis.Application.Modules.Quotes.Queries;

public sealed record GetQuoteStatusQuery(Guid QuoteRequestId) : IRequest<QuoteRequestDto?>;
