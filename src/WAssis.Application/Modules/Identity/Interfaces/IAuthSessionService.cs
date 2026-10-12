namespace WAssis.Application.Modules.Identity.Interfaces;

public interface IAuthSessionService
{
    Task RegisterAsync(string token, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetAuthorizedBranchesAsync(string jti, string userId, string tenantId, CancellationToken cancellationToken);
    Task RevokeAsync(string jti, CancellationToken cancellationToken);
}
