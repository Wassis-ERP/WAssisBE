using MediatR;
using WAssis.Application.Modules.Customers.Dtos;

namespace WAssis.Application.Modules.Customers.Commands;

public sealed record UpdateInsuredPersonCommand(
    Guid Id,
    string? OfficeBranchId,
    string Name,
    string? PersonType,
    string? Status,
    string? DocumentNumber,
    string? Email,
    string? PhoneNumber,
    DateTime? BirthDateUtc,
    string? TradeName,
    string? Gender,
    string? MaritalStatus,
    string? CompanySize,
    string? Cnae,
    string? Website,
    string? PostalCode,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State,
    string? Notes,
    string? ProducerId,
    string? ManagerId,
    string? ChatwootId,
    bool LgpdAuthorized) : IRequest<InsuredPersonDto?>;
