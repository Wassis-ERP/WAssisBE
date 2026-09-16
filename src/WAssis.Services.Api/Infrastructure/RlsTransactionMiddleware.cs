using Microsoft.EntityFrameworkCore;
using WAssis.Application.Abstractions;
using WAssis.Infra.Data.Context;

namespace WAssis.Services.Api.Infrastructure;

public sealed class RlsTransactionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        WAssisDbContext db,
        ICurrentUserContext currentUser)
    {
        if (!currentUser.IsAuthenticated
            || !Guid.TryParse(currentUser.TenantId, out var tenantId)
            || !Guid.TryParse(currentUser.UserId, out var userId))
        {
            await next(context);
            return;
        }

        var branchId = Guid.TryParse(currentUser.BranchId, out var parsedBranch)
            ? parsedBranch.ToString()
            : string.Empty;
        await using var transaction = await db.Database.BeginTransactionAsync(context.RequestAborted);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.tenant_id', {tenantId.ToString()}, true), set_config('app.branch_id', {branchId}, true), set_config('app.user_id', {userId.ToString()}, true)",
            context.RequestAborted);
        try
        {
            await next(context);
            if (context.Response.StatusCode >= StatusCodes.Status400BadRequest)
                await transaction.RollbackAsync(CancellationToken.None);
            else
                await transaction.CommitAsync(context.RequestAborted);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
