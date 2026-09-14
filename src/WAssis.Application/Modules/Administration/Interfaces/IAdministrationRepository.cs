using WAssis.Application.Modules.Administration.Dtos;

namespace WAssis.Application.Modules.Administration.Interfaces;

public interface IAdministrationRepository
{
    Task<OrganizationDto?> GetOrganizationAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<OrganizationDto?> UpdateOrganizationAsync(Guid tenantId, OrganizationUpdateDto update, CancellationToken cancellationToken);
    Task<OrganizationStatisticsDto> GetStatisticsAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdministrationBranchDto>> ListBranchesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<AdministrationBranchDto> CreateBranchAsync(Guid tenantId, AdministrationBranchWriteDto create, CancellationToken cancellationToken);
    Task<AdministrationBranchDto?> UpdateBranchAsync(Guid tenantId, Guid branchId, AdministrationBranchWriteDto update, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AdministrationUserDto>> ListUsersAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<AdministrationUserDto> InviteUserAsync(Guid tenantId, UserInvitationDto invitation, CancellationToken cancellationToken);
    Task<bool> SetUserStatusAsync(Guid tenantId, Guid userId, bool isActive, Guid actorUserId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<UserBranchAccessDto>> ListUserBranchAccessAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken);
    Task<UserBranchAccessDto?> UpsertUserBranchAccessAsync(Guid tenantId, Guid userId, Guid branchId, UserBranchAccessUpdateDto update, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AccessProfileDto>> ListAccessProfilesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<AccessProfileDto> CreateAccessProfileAsync(Guid tenantId, AccessProfileCreateDto create, CancellationToken cancellationToken);
    Task<AccessProfileDto?> UpdateAccessProfileAsync(Guid tenantId, Guid profileId, AccessProfileUpdateDto update, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<AccessPermissionDto>> ListPermissionsAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<AccessPermissionDto?> UpdatePermissionAsync(Guid tenantId, Guid permissionId, AccessPermissionUpdateDto update, CancellationToken cancellationToken);
}
