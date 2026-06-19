namespace WAssis.Application.Modules.Core.Dtos;

public sealed record CoreBranchDto(
    Guid Id,
    Guid? ParentBranchId,
    string? Name,
    string? DocumentNumber,
    bool IsActive);
