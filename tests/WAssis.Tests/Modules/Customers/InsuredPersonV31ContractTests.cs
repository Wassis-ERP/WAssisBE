using WAssis.Application.Modules.Customers.Commands;
using WAssis.Application.Modules.Customers.Validators;
using WAssis.Domain.Modules.Customers.Entities;

namespace WAssis.Tests.Modules.Customers;

public sealed class InsuredPersonV31ContractTests
{
    [Fact]
    public void Create_PersistsAndNormalizesV31Fields()
    {
        var lgpdAt = new DateTime(2026, 9, 12, 10, 30, 0, DateTimeKind.Utc);

        var insured = InsuredPerson.Create(
            tenantId: "tenant-1",
            officeBranchId: "branch-1",
            name: "Maria Souza",
            personType: "PF",
            status: "Ativo",
            documentNumber: "12345678900",
            email: "maria@example.test",
            phoneNumber: "1133334444",
            birthDateUtc: null,
            tradeName: null,
            gender: null,
            maritalStatus: null,
            companySize: null,
            cnae: null,
            website: null,
            postalCode: null,
            street: null,
            number: null,
            complement: null,
            neighborhood: null,
            city: null,
            state: null,
            notes: null,
            producerId: null,
            managerId: null,
            chatwootId: null,
            lgpdAuthorized: true,
            createdBy: "user-1",
            socialName: " Maria ",
            identityDocument: " RG-1 ",
            municipalRegistration: null,
            economicActivity: null,
            profession: " Corretora ",
            monthlyIncome: 9500.50m,
            driverLicenseNumber: " CNH-1 ",
            driverLicenseCategory: " B ",
            driverLicenseExpirationDate: new DateTime(2030, 1, 2, 15, 0, 0, DateTimeKind.Utc),
            mobilePhoneNumber: "11999990000",
            secondaryPhoneNumber: "1144445555",
            whatsAppNumber: "11999990000",
            country: "Brasil",
            lgpdAuthorizedAtUtc: lgpdAt,
            importOrigin: "MANUAL");

        Assert.Equal("Maria", insured.SocialName);
        Assert.Equal("RG-1", insured.IdentityDocument);
        Assert.Equal("Corretora", insured.Profession);
        Assert.Equal(9500.50m, insured.MonthlyIncome);
        Assert.Equal(new DateTime(2030, 1, 2), insured.DriverLicenseExpirationDate);
        Assert.Equal("11999990000", insured.MobilePhoneNumber);
        Assert.Equal(lgpdAt, insured.LgpdAuthorizedAtUtc);
        Assert.Equal("MANUAL", insured.ImportOrigin);
    }

    [Fact]
    public void Validator_RejectsNegativeIncomeAndOversizedV31Fields()
    {
        var command = new CreateInsuredPersonCommand(
            OfficeBranchId: "branch-1",
            Name: "Maria",
            PersonType: "PF",
            Status: "Ativo",
            DocumentNumber: null,
            Email: null,
            PhoneNumber: null,
            BirthDateUtc: null,
            TradeName: null,
            Gender: null,
            MaritalStatus: null,
            CompanySize: null,
            Cnae: null,
            Website: null,
            PostalCode: null,
            Street: null,
            Number: null,
            Complement: null,
            Neighborhood: null,
            City: null,
            State: null,
            Notes: null,
            ProducerId: null,
            ManagerId: null,
            ChatwootId: null,
            LgpdAuthorized: false,
            SocialName: new string('x', 201),
            MonthlyIncome: -1m);

        var result = new CreateInsuredPersonCommandValidator().Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.SocialName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.MonthlyIncome));
    }
}
