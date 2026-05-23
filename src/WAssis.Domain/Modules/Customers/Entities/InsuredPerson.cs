using WAssis.Domain.Core.Entities;

namespace WAssis.Domain.Modules.Customers.Entities;

public class InsuredPerson : AggregateRoot
{
    public string TenantId { get; private set; } = string.Empty;
    public string? OfficeBranchId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string PersonType { get; private set; } = "PF";
    public string Status { get; private set; } = "Ativo";
    public string? DocumentNumber { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateTime? BirthDateUtc { get; private set; }
    public string? TradeName { get; private set; }
    public string? Gender { get; private set; }
    public string? MaritalStatus { get; private set; }
    public string? CompanySize { get; private set; }
    public string? Cnae { get; private set; }
    public string? Website { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Street { get; private set; }
    public string? Number { get; private set; }
    public string? Complement { get; private set; }
    public string? Neighborhood { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? Notes { get; private set; }
    public string? ProducerId { get; private set; }
    public string? ManagerId { get; private set; }
    public string? ChatwootId { get; private set; }
    public bool LgpdAuthorized { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private InsuredPerson()
    {
    }

    private InsuredPerson(
        Guid id,
        string tenantId,
        string? officeBranchId,
        string name,
        string personType,
        string status,
        string? documentNumber,
        string? email,
        string? phoneNumber,
        DateTime? birthDateUtc,
        string? tradeName,
        string? gender,
        string? maritalStatus,
        string? companySize,
        string? cnae,
        string? website,
        string? postalCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state,
        string? notes,
        string? producerId,
        string? managerId,
        string? chatwootId,
        bool lgpdAuthorized,
        string? createdBy)
    {
        Id = id;
        TenantId = tenantId;
        OfficeBranchId = Normalize(officeBranchId);
        CreatedAtUtc = DateTime.UtcNow;
        CreatedBy = createdBy;
        Update(
            officeBranchId,
            name,
            personType,
            status,
            documentNumber,
            email,
            phoneNumber,
            birthDateUtc,
            tradeName,
            gender,
            maritalStatus,
            companySize,
            cnae,
            website,
            postalCode,
            street,
            number,
            complement,
            neighborhood,
            city,
            state,
            notes,
            producerId,
            managerId,
            chatwootId,
            lgpdAuthorized);
        UpdatedAtUtc = CreatedAtUtc;
    }

    public static InsuredPerson Create(
        string tenantId,
        string? officeBranchId,
        string name,
        string personType,
        string status,
        string? documentNumber,
        string? email,
        string? phoneNumber,
        DateTime? birthDateUtc,
        string? tradeName,
        string? gender,
        string? maritalStatus,
        string? companySize,
        string? cnae,
        string? website,
        string? postalCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state,
        string? notes,
        string? producerId,
        string? managerId,
        string? chatwootId,
        bool lgpdAuthorized,
        string? createdBy)
    {
        return new InsuredPerson(
            Guid.NewGuid(),
            tenantId,
            officeBranchId,
            name,
            personType,
            status,
            documentNumber,
            email,
            phoneNumber,
            birthDateUtc,
            tradeName,
            gender,
            maritalStatus,
            companySize,
            cnae,
            website,
            postalCode,
            street,
            number,
            complement,
            neighborhood,
            city,
            state,
            notes,
            producerId,
            managerId,
            chatwootId,
            lgpdAuthorized,
            createdBy);
    }

    public void Update(
        string? officeBranchId,
        string name,
        string personType,
        string status,
        string? documentNumber,
        string? email,
        string? phoneNumber,
        DateTime? birthDateUtc,
        string? tradeName,
        string? gender,
        string? maritalStatus,
        string? companySize,
        string? cnae,
        string? website,
        string? postalCode,
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state,
        string? notes,
        string? producerId,
        string? managerId,
        string? chatwootId,
        bool lgpdAuthorized)
    {
        OfficeBranchId = Normalize(officeBranchId);
        Name = name.Trim();
        PersonType = string.IsNullOrWhiteSpace(personType) ? "PF" : personType.Trim();
        Status = string.IsNullOrWhiteSpace(status) ? "Ativo" : status.Trim();
        DocumentNumber = Normalize(documentNumber);
        Email = Normalize(email);
        PhoneNumber = Normalize(phoneNumber);
        BirthDateUtc = birthDateUtc;
        TradeName = Normalize(tradeName);
        Gender = Normalize(gender);
        MaritalStatus = Normalize(maritalStatus);
        CompanySize = Normalize(companySize);
        Cnae = Normalize(cnae);
        Website = Normalize(website);
        PostalCode = Normalize(postalCode);
        Street = Normalize(street);
        Number = Normalize(number);
        Complement = Normalize(complement);
        Neighborhood = Normalize(neighborhood);
        City = Normalize(city);
        State = Normalize(state);
        Notes = Normalize(notes);
        ProducerId = Normalize(producerId);
        ManagerId = Normalize(managerId);
        ChatwootId = Normalize(chatwootId);
        LgpdAuthorized = lgpdAuthorized;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
