using MediatR;
using WAssis.Application.Modules.Customers.Dtos;
using WAssis.Application.Modules.Customers.Interfaces;

namespace WAssis.Application.Modules.Customers.Queries;

public sealed class ListInsuredPeopleQueryHandler(IInsuredPersonReadRepository repository)
    : IRequestHandler<ListInsuredPeopleQuery, IReadOnlyCollection<InsuredPersonDto>>
{
    public async Task<IReadOnlyCollection<InsuredPersonDto>> Handle(ListInsuredPeopleQuery request, CancellationToken cancellationToken)
    {
        var insuredPeople = await repository.ListAsync(request.Search, request.Status, cancellationToken);
        return insuredPeople.Select(InsuredPersonMappings.ToDto).ToArray();
    }
}
