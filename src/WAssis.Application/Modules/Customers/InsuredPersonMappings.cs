using WAssis.Application.Modules.Customers.Dtos;
using WAssis.Domain.Modules.Customers.Entities;

namespace WAssis.Application.Modules.Customers;

internal static class InsuredPersonMappings
{
    public static InsuredPersonDto ToDto(InsuredPerson insuredPerson)
    {
        return new InsuredPersonDto(
            insuredPerson.Id,
            insuredPerson.OfficeBranchId,
            insuredPerson.Name,
            insuredPerson.PersonType,
            insuredPerson.Status,
            insuredPerson.DocumentNumber,
            insuredPerson.Email,
            insuredPerson.PhoneNumber,
            insuredPerson.BirthDateUtc,
            insuredPerson.TradeName,
            insuredPerson.Gender,
            insuredPerson.MaritalStatus,
            insuredPerson.CompanySize,
            insuredPerson.Cnae,
            insuredPerson.Website,
            insuredPerson.PostalCode,
            insuredPerson.Street,
            insuredPerson.Number,
            insuredPerson.Complement,
            insuredPerson.Neighborhood,
            insuredPerson.City,
            insuredPerson.State,
            insuredPerson.Notes,
            insuredPerson.ProducerId,
            insuredPerson.ManagerId,
            insuredPerson.ChatwootId,
            insuredPerson.LgpdAuthorized,
            insuredPerson.CreatedBy,
            insuredPerson.CreatedAtUtc,
            insuredPerson.UpdatedAtUtc,
            insuredPerson.SocialName,
            insuredPerson.IdentityDocument,
            insuredPerson.MunicipalRegistration,
            insuredPerson.EconomicActivity,
            insuredPerson.Profession,
            insuredPerson.MonthlyIncome,
            insuredPerson.DriverLicenseNumber,
            insuredPerson.DriverLicenseCategory,
            insuredPerson.DriverLicenseExpirationDate,
            insuredPerson.MobilePhoneNumber,
            insuredPerson.SecondaryPhoneNumber,
            insuredPerson.WhatsAppNumber,
            insuredPerson.Country,
            insuredPerson.LgpdAuthorizedAtUtc,
            insuredPerson.ImportOrigin);
    }
}
