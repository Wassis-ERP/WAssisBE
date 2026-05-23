using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Customers.Dtos;
using WAssis.Application.Modules.Customers.Interfaces;

namespace WAssis.Application.Modules.Customers.Commands;

public sealed class UpdateInsuredPersonCommandHandler(
    IInsuredPersonRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<UpdateInsuredPersonCommand, InsuredPersonDto?>
{
    public async Task<InsuredPersonDto?> Handle(UpdateInsuredPersonCommand request, CancellationToken cancellationToken)
    {
        var insuredPerson = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (insuredPerson is null)
        {
            return null;
        }

        insuredPerson.Update(
            currentUserContext.ResolveBranchIdForWrite(request.OfficeBranchId),
            request.Name,
            request.PersonType ?? "PF",
            request.Status ?? "Ativo",
            request.DocumentNumber,
            request.Email,
            request.PhoneNumber,
            request.BirthDateUtc,
            request.TradeName,
            request.Gender,
            request.MaritalStatus,
            request.CompanySize,
            request.Cnae,
            request.Website,
            request.PostalCode,
            request.Street,
            request.Number,
            request.Complement,
            request.Neighborhood,
            request.City,
            request.State,
            request.Notes,
            request.ProducerId,
            request.ManagerId,
            request.ChatwootId,
            request.LgpdAuthorized);

        await repository.SaveChangesAsync(cancellationToken);

        return InsuredPersonMappings.ToDto(insuredPerson);
    }
}
