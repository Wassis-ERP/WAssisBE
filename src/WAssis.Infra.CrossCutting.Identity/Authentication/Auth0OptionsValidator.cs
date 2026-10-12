using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WAssis.Infra.CrossCutting.Identity.Authentication;

public sealed class Auth0OptionsValidator(IHostEnvironment environment) : IValidateOptions<Auth0Options>
{
    public ValidateOptionsResult Validate(string? name, Auth0Options options)
    {
        if (!options.Enabled)
            return environment.IsProduction() && options.RequiredInProduction
                ? ValidateOptionsResult.Fail("Identity:Auth0:Enabled must be true in Production.")
                : ValidateOptionsResult.Success;

        var failures = new List<string>();
        if (!IsHttpsAbsolute(options.Authority)) failures.Add("Identity:Auth0:Authority must be an absolute HTTPS URL.");
        if (string.IsNullOrWhiteSpace(options.ClientId)) failures.Add("Identity:Auth0:ClientId is required.");
        if (string.IsNullOrWhiteSpace(options.ClientSecret) || options.ClientSecret.StartsWith("__", StringComparison.Ordinal))
            failures.Add("Identity:Auth0:ClientSecret must come from an external secret store.");
        if (!IsAllowedRedirect(options.FrontendCallbackUrl)) failures.Add("Identity:Auth0:FrontendCallbackUrl must be an absolute HTTPS URL outside Development.");
        if (!IsAllowedRedirect(options.FrontendLogoutUrl)) failures.Add("Identity:Auth0:FrontendLogoutUrl must be an absolute HTTPS URL outside Development.");
        if (options.AccessTokenMinutes is < 1 or > 15) failures.Add("Identity:Auth0:AccessTokenMinutes must be between 1 and 15.");
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsHttpsAbsolute(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            && string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment);

    private bool IsAllowedRedirect(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || !string.IsNullOrEmpty(uri.UserInfo)
            || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)) return false;
        return uri.Scheme == Uri.UriSchemeHttps
            || environment.IsDevelopment() && uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback;
    }
}
