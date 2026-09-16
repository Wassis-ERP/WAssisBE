namespace WAssis.Infra.Data.Configuration;

public static class ExternalEndpointPolicy
{
    public static Uri RequireHttpsAllowlisted(string value, IEnumerable<string> allowedHosts)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
            || uri.IsLoopback || !string.IsNullOrEmpty(uri.UserInfo))
            throw new InvalidOperationException("External integration endpoint must use an absolute HTTPS URL without embedded credentials.");

        var allowlist = allowedHosts
            .Where(static host => !string.IsNullOrWhiteSpace(host))
            .Select(static host => host.Trim().TrimEnd('.'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!allowlist.Contains(uri.IdnHost.TrimEnd('.')))
            throw new InvalidOperationException("External integration endpoint host is not allowlisted.");
        return uri;
    }
}
