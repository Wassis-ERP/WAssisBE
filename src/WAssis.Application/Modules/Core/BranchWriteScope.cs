using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Core.Interfaces;

namespace WAssis.Application.Modules.Core;

public static class BranchWriteScope
{
    public static async Task<string> ValidateAsync(ICurrentUserContext user, ICoreBranchReadRepository branches, string? requested, CancellationToken cancellationToken)
    {
        var branchId = user.ResolveBranchIdForWrite(requested);
        if (!user.IsAuthenticated || !Guid.TryParse(user.TenantId, out var tenant) || !Guid.TryParse(branchId, out var branch))
            throw new UnauthorizedAccessException("Grupo e corretora válidos são obrigatórios.");
        // Even allBranches means all branches of this tenant, never of another tenant.
        var available = await branches.ListAsync(tenant, [branch], false, cancellationToken);
        if (!available.Any(x => x.Id == branch && x.IsActive))
            throw new UnauthorizedAccessException("Corretora indisponível neste grupo.");
        return branch.ToString();
    }
}
