using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;

namespace WAssis.Application.Modules.Quotes.Queries;

public sealed class ListQuoteCalculationsQueryHandler(
    IQuoteRequestRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<ListQuoteCalculationsQuery, IReadOnlyCollection<QuoteRequestDto>>
{
    public async Task<IReadOnlyCollection<QuoteRequestDto>> Handle(
        ListQuoteCalculationsQuery request,
        CancellationToken cancellationToken)
    {
        var branchId = string.IsNullOrWhiteSpace(request.OfficeBranchId)
            ? null
            : request.OfficeBranchId.Trim();

        if (branchId is not null && !currentUserContext.CanAccessBranch(branchId))
        {
            throw new UnauthorizedAccessException("Usuario nao possui acesso a filial informada.");
        }

        var requests = await repository.ListAsync(request.OpportunityId, branchId, cancellationToken);
        return requests.Select(QuoteRequestMappings.ToDto).ToArray();
    }
}
