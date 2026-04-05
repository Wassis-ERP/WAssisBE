using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WAssis.Infra.CrossCutting.Identity.Jwt;

public sealed class JwtAccessOptionsValidator(IHostEnvironment hostEnvironment) : IValidateOptions<JwtAccessOptions>
{
    private static readonly string[] PlaceholderSigningKeys =
    [
        "change-this-signing-key-for-real-environments-12345",
        "__configure_in_user_secrets_or_env__"
    ];

    public ValidateOptionsResult Validate(string? name, JwtAccessOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Identity:Jwt:Issuer must be configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Identity:Jwt:Audience must be configured.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey) || options.SigningKey.Trim().Length < 32)
        {
            failures.Add("Identity:Jwt:SigningKey must contain at least 32 characters.");
        }

        var usesPlaceholderKey = PlaceholderSigningKeys.Contains(options.SigningKey, StringComparer.Ordinal);
        if (!hostEnvironment.IsDevelopment() && usesPlaceholderKey)
        {
            failures.Add("Identity:Jwt:SigningKey cannot use the repository placeholder outside Development.");
        }

        if (!hostEnvironment.IsDevelopment() && !options.RequireHttpsMetadata)
        {
            failures.Add("Identity:Jwt:RequireHttpsMetadata must be enabled outside Development.");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
