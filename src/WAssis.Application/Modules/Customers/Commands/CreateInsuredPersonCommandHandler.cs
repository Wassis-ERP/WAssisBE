using MediatR;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Customers.Dtos;
using WAssis.Application.Modules.Customers.Interfaces;
using WAssis.Domain.Modules.Customers.Entities;

namespace WAssis.Application.Modules.Customers.Commands;

public sealed class CreateInsuredPersonCommandHandler(
    IInsuredPersonRepository repository,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<CreateInsuredPersonCommand, InsuredPersonDto>
{
    public async Task<InsuredPersonDto> Handle(CreateInsuredPersonCommand request, CancellationToken cancellationToken)
    {
        var insuredPerson = InsuredPerson.Create(
            currentUserContext.ResolveTenantIdOrPlatform(),
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
            request.LgpdAuthorized,
            currentUserContext.UserId,
            request.SocialName,
            request.IdentityDocument,
            request.MunicipalRegistration,
            request.EconomicActivity,
            request.Profession,
            request.MonthlyIncome,
            request.DriverLicenseNumber,
            request.DriverLicenseCategory,
            request.DriverLicenseExpirationDate,
            request.MobilePhoneNumber,
            request.SecondaryPhoneNumber,
            request.WhatsAppNumber,
            request.Country,
            request.LgpdAuthorizedAtUtc,
            request.ImportOrigin);

        await repository.AddAsync(insuredPerson, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return InsuredPersonMappings.ToDto(insuredPerson);
    }
}
