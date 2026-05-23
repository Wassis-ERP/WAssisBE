using MediatR;
using WAssis.Application.Modules.Customers.Dtos;
using WAssis.Application.Modules.Customers.Interfaces;

namespace WAssis.Application.Modules.Customers.Queries;

public sealed class GetInsuredPersonQueryHandler(IInsuredPersonRepository repository)
    : IRequestHandler<GetInsuredPersonQuery, InsuredPersonDto?>
{
    public async Task<InsuredPersonDto?> Handle(GetInsuredPersonQuery request, CancellationToken cancellationToken)
    {
        var insuredPerson = await repository.GetByIdAsync(request.Id, cancellationToken);
        return insuredPerson is null ? null : InsuredPersonMappings.ToDto(insuredPerson);
    }
}
