namespace WAssis.Application.Abstractions;

public static class CurrentUserContextExtensions
{
    public static string ResolveTenantIdOrPlatform(this ICurrentUserContext currentUserContext)
    {
        if (currentUserContext.IsAuthenticated && !string.IsNullOrWhiteSpace(currentUserContext.TenantId))
        {
            return currentUserContext.TenantId.Trim();
        }

        throw new UnauthorizedAccessException("Contexto autenticado de tenant obrigatório.");
    }

    public static string? ResolveBranchIdForWrite(this ICurrentUserContext currentUserContext, string? requestedBranchId = null)
    {
        var branchId = string.IsNullOrWhiteSpace(requestedBranchId)
            ? currentUserContext.BranchId
            : requestedBranchId.Trim();

        if (string.IsNullOrWhiteSpace(branchId))
        {
            return null;
        }

        if (!currentUserContext.CanAccessBranch(branchId))
        {
            throw new UnauthorizedAccessException("Usuario nao possui acesso a filial informada.");
        }

        return branchId;
    }

    public static bool CanAccessBranch(this ICurrentUserContext currentUserContext, string? branchId)
    {
        if (string.IsNullOrWhiteSpace(branchId) || currentUserContext.HasAllBranchesAccess)
        {
            return true;
        }

        return string.Equals(currentUserContext.BranchId, branchId, StringComparison.OrdinalIgnoreCase)
            || currentUserContext.BranchIds.Contains(branchId, StringComparer.OrdinalIgnoreCase);
    }
}
