using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Administration.Dtos;
using WAssis.Application.Modules.Administration.Interfaces;
using WAssis.Services.Api.Modules.Administration.Controllers;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Administration;

public sealed class AdministrationControllerTests
{
    [Fact]
    public async Task ListUsers_RejectsMissingUuidScope()
    {
        var controller = Create(new FakeCurrentUserContext { IsAuthenticated = true, TenantId = "tenant", UserId = "user" });

        var result = await controller.ListUsers(default);

        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsType<ObjectResult>(result).StatusCode);
    }

    [Fact]
    public async Task InviteUser_ValidatesEmailBeforePersistence()
    {
        var repository = new StubAdministrationRepository();
        var controller = Create(ValidUser(), repository);

        var result = await controller.InviteUser(new UserInvitationDto("Pessoa", "email-invalido"), default);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Null(repository.Invited);
    }

    [Fact]
    public async Task UpdatePermission_RejectsUnknownScope()
    {
        var repository = new StubAdministrationRepository();
        var controller = Create(ValidUser(), repository);
        var update = new AccessPermissionUpdateDto("GLOBAL", true, false, false, false, false, false);

        var result = await controller.UpdatePermission(Guid.NewGuid(), update, default);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.False(repository.PermissionUpdated);
    }

    private static FakeCurrentUserContext ValidUser() => new()
    {
        IsAuthenticated = true,
        TenantId = Guid.NewGuid().ToString(),
        UserId = Guid.NewGuid().ToString(),
    };

    private static AdministrationController Create(
        FakeCurrentUserContext user,
        StubAdministrationRepository? repository = null)
    {
        var controller = new AdministrationController(user, repository ?? new(), new NullAuditTrailWriter())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        return controller;
    }

    private sealed class NullAuditTrailWriter : IAuditTrailWriter
    {
        public Task WriteAsync(string correlationId, string module, string action, string entityType, string entityId,
            string? notes, CancellationToken cancellationToken, string? tenantId = null) => Task.CompletedTask;
    }

    private sealed class StubAdministrationRepository : IAdministrationRepository
    {
        public Task<bool> HasAdministrationPermissionAsync(Guid tenantId, Guid userId, bool manage, CancellationToken cancellationToken) => Task.FromResult(true);
        public UserInvitationDto? Invited { get; private set; }
        public bool PermissionUpdated { get; private set; }
        public Task<OrganizationDto?> GetOrganizationAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<OrganizationDto?>(null);
        public Task<OrganizationDto?> UpdateOrganizationAsync(Guid tenantId, OrganizationUpdateDto update, CancellationToken cancellationToken) => Task.FromResult<OrganizationDto?>(null);
        public Task<OrganizationStatisticsDto> GetStatisticsAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult(new OrganizationStatisticsDto(0, 0, 0, 0, 0, 0, 0, 0));
        public Task<IReadOnlyCollection<AdministrationBranchDto>> ListBranchesAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<AdministrationBranchDto>>([]);
        public Task<AdministrationBranchDto> CreateBranchAsync(Guid tenantId, AdministrationBranchWriteDto create, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AdministrationBranchDto?> UpdateBranchAsync(Guid tenantId, Guid branchId, AdministrationBranchWriteDto update, CancellationToken cancellationToken) => Task.FromResult<AdministrationBranchDto?>(null);
        public Task<IReadOnlyCollection<AdministrationUserDto>> ListUsersAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<AdministrationUserDto>>([]);
        public Task<AdministrationUserDto> InviteUserAsync(Guid tenantId, UserInvitationDto invitation, CancellationToken cancellationToken)
        {
            Invited = invitation;
            return Task.FromResult(new AdministrationUserDto(Guid.NewGuid(), invitation.Name, invitation.Email, null, "ATIVO", true, "PENDENTE", null, null, 0, null));
        }
        public Task<bool> SetUserStatusAsync(Guid tenantId, Guid userId, bool isActive, Guid actorUserId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<IReadOnlyCollection<UserBranchAccessDto>> ListUserBranchAccessAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<UserBranchAccessDto>>([]);
        public Task<UserBranchAccessDto?> UpsertUserBranchAccessAsync(Guid tenantId, Guid userId, Guid branchId, UserBranchAccessUpdateDto update, CancellationToken cancellationToken) => Task.FromResult<UserBranchAccessDto?>(null);
        public Task<IReadOnlyCollection<AccessProfileDto>> ListAccessProfilesAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<AccessProfileDto>>([]);
        public Task<AccessProfileDto> CreateAccessProfileAsync(Guid tenantId, AccessProfileCreateDto create, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<AccessProfileDto?> UpdateAccessProfileAsync(Guid tenantId, Guid profileId, AccessProfileUpdateDto update, CancellationToken cancellationToken) => Task.FromResult<AccessProfileDto?>(null);
        public Task<IReadOnlyCollection<AccessPermissionDto>> ListPermissionsAsync(Guid tenantId, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyCollection<AccessPermissionDto>>([]);
        public Task<AccessPermissionDto?> UpdatePermissionAsync(Guid tenantId, Guid permissionId, AccessPermissionUpdateDto update, CancellationToken cancellationToken)
        {
            PermissionUpdated = true;
            return Task.FromResult<AccessPermissionDto?>(null);
        }
    }
}
