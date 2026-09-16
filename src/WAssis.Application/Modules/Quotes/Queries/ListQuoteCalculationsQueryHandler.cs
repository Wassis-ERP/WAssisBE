using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Quotes.Dtos;
using WAssis.Application.Modules.Quotes.Interfaces;

namespace WAssis.Application.Modules.Quotes.Queries;

public sealed class ListQuoteCalculationsQueryHandler(
    IQuoteRequestRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<ListQuoteCalculationsQuery, PagedResult<QuoteRequestDto>>
{
    public async Task<PagedResult<QuoteRequestDto>> Handle(
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

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var requests = await repository.ListAsync(request.OpportunityId, branchId, page, pageSize, cancellationToken);
        return new PagedResult<QuoteRequestDto>(
            requests.Items.Select(QuoteRequestMappings.ToDto).ToArray(),
            requests.Page,
            requests.PageSize,
            requests.TotalCount);
    }
}
