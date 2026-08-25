using MediatR;
using WAssis.Application.Modules.Quotes.Dtos;

namespace WAssis.Application.Modules.Quotes.Queries;

public sealed record ListQuoteCalculationsQuery(
    Guid? OpportunityId,
    string? OfficeBranchId) : IRequest<IReadOnlyCollection<QuoteRequestDto>>;
