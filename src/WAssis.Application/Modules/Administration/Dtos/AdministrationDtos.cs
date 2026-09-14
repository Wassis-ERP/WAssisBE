namespace WAssis.Application.Modules.Administration.Dtos;

public sealed record OrganizationDto(
    Guid Id,
    string? LegalName,
    string? TradeName,
    string? DocumentNumber,
    string? Email,
    string? Phone,
    string? Mobile,
    string? Website,
    string? TimeZone,
    string? DefaultCurrency,
    string? Status,
    bool IsActive);

public sealed record OrganizationUpdateDto(
    string? LegalName,
    string? TradeName,
    string? DocumentNumber,
    string? Email,
    string? Phone,
    string? Mobile,
    string? Website,
    string? TimeZone,
    string? DefaultCurrency);

public sealed record OrganizationStatisticsDto(
    int ActiveBranches,
    int ActiveUsers,
    int InactiveUsers,
    int PendingInvitations,
    int InsuredPeople,
    int OpenOpportunities,
    int WonOpportunities,
    int LostOpportunities);

public sealed record AdministrationBranchDto(
    Guid Id,
    Guid TenantId,
    Guid? ParentBranchId,
    string? LegalName,
    string? TradeName,
    string? DocumentNumber,
    string? Susep,
    decimal? TaxPercentage,
    bool LgpdAccepted,
    DateTimeOffset? LgpdAcceptedAt,
    string? Manager,
    Guid? ManagerId,
    string? Contact,
    string? Website,
    string? Email,
    string? Phone,
    string? Mobile,
    string? SecondaryPhone,
    string? StateRegistration,
    string? MunicipalRegistration,
    string? TaxRegime,
    decimal? IssPercentage,
    string? BrokerageCode,
    string? ExternalCode,
    string? IbgeCityCode,
    string? Country,
    string? BusinessHours,
    string? Notes,
    string? PostalCode,
    string? Address,
    string? Number,
    string? Complement,
    string? District,
    string? City,
    string? State,
    bool IsActive);

public sealed record AdministrationBranchWriteDto(
    Guid? ParentBranchId,
    string? LegalName,
    string? TradeName,
    string? DocumentNumber,
    string? Susep,
    decimal? TaxPercentage,
    bool LgpdAccepted,
    DateTimeOffset? LgpdAcceptedAt,
    string? Manager,
    Guid? ManagerId,
    string? Contact,
    string? Website,
    string? Email,
    string? Phone,
    string? Mobile,
    string? SecondaryPhone,
    string? StateRegistration,
    string? MunicipalRegistration,
    string? TaxRegime,
    decimal? IssPercentage,
    string? BrokerageCode,
    string? ExternalCode,
    string? IbgeCityCode,
    string? Country,
    string? BusinessHours,
    string? Notes,
    string? PostalCode,
    string? Address,
    string? Number,
    string? Complement,
    string? District,
    string? City,
    string? State,
    bool IsActive);

public sealed record AdministrationUserDto(
    Guid Id,
    string Name,
    string Email,
    string? AvatarUrl,
    string Status,
    bool IsActive,
    string? InvitationStatus,
    DateTimeOffset? InvitationSentAt,
    DateTimeOffset? LastAccessAt,
    int BranchCount,
    string? PrimaryAccessProfile);

public sealed record UserInvitationDto(string Name, string Email);

public sealed record UserStatusUpdateDto(bool IsActive);

public sealed record UserBranchAccessDto(
    Guid Id,
    Guid UserId,
    Guid BranchId,
    Guid AccessProfileId,
    bool IsPrimary,
    bool IsActive,
    DateOnly? StartsOn,
    DateOnly? EndsOn);

public sealed record UserBranchAccessUpdateDto(
    Guid AccessProfileId,
    bool IsPrimary,
    bool IsActive,
    DateOnly? StartsOn,
    DateOnly? EndsOn);

public sealed record AccessProfileDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    string? AccessLevel,
    int? Order,
    bool IsActive);

public sealed record AccessProfileCreateDto(string Name, string? Description, string? AccessLevel);

public sealed record AccessProfileUpdateDto(string Name, string? Description, string? AccessLevel, bool IsActive);

public sealed record AccessPermissionDto(
    Guid Id,
    Guid AccessProfileId,
    string Module,
    string Scope,
    bool CanRead,
    bool CanCreate,
    bool CanUpdate,
    bool CanDelete,
    bool CanExport,
    bool CanManage);

public sealed record AccessPermissionUpdateDto(
    string Scope,
    bool CanRead,
    bool CanCreate,
    bool CanUpdate,
    bool CanDelete,
    bool CanExport,
    bool CanManage);
