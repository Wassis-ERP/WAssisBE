using MediatR;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.Quotes.Queries;

public sealed record ListQuoteCalculationsQuery(
    Guid? OpportunityId,
    string? OfficeBranchId,
    int Page,
    int PageSize) : IRequest<PagedResult<QuoteRequestDto>>;
