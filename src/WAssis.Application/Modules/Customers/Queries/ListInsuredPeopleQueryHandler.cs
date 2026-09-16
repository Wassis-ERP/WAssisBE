using MediatR;
using WAssis.Application.Modules.Customers.Dtos;
using WAssis.Application.Modules.Customers.Interfaces;
using WAssis.Application.Abstractions;

namespace WAssis.Application.Modules.Customers.Queries;

public sealed class ListInsuredPeopleQueryHandler(IInsuredPersonReadRepository repository)
    : IRequestHandler<ListInsuredPeopleQuery, PagedResult<InsuredPersonDto>>
{
    public async Task<PagedResult<InsuredPersonDto>> Handle(ListInsuredPeopleQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var insuredPeople = await repository.ListAsync(request.Search, request.Status, page, pageSize, cancellationToken);
        return new PagedResult<InsuredPersonDto>(insuredPeople.Items.Select(InsuredPersonMappings.ToDto).ToArray(), page, pageSize, insuredPeople.TotalCount);
    }
}
