using WAssis.Domain.Core.Tenancy;

namespace WAssis.Application.Abstractions;

public static class CurrentUserContextExtensions
{
    public static string ResolveTenantIdOrPlatform(this ICurrentUserContext currentUserContext)
    {
        if (!string.IsNullOrWhiteSpace(currentUserContext.TenantId))
        {
            return currentUserContext.TenantId.Trim();
        }

        if (!string.IsNullOrWhiteSpace(currentUserContext.BrokerageId))
        {
            return currentUserContext.BrokerageId.Trim();
        }

        return TenantConstants.PlatformTenantId;
    }
}
