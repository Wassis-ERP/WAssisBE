using System.Buffers.Binary;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WAssis.Infra.CrossCutting.Identity.Models;

namespace WAssis.Infra.CrossCutting.Identity.Authentication;

public sealed class HomologationAuthOptionsValidator(IHostEnvironment environment) : IValidateOptions<HomologationAuthOptions>
{
    private static readonly string[] AllowedRoles =
    [AppRoles.BrokerageOwner, AppRoles.BrokerageAdmin, AppRoles.BrokerageManager, AppRoles.BrokerageSeller, AppRoles.BrokerageAgent];

    public ValidateOptionsResult Validate(string? name, HomologationAuthOptions options)
    {
        if (!options.Enabled) return ValidateOptionsResult.Success;
        var failures = new List<string>();
        if (!environment.IsStaging()) failures.Add("Identity:HomologationAuth is restricted to Staging.");
        if (options.TokenExpirationMinutes is < 1 or > 15) failures.Add("Homologation tokens must expire within 1 to 15 minutes.");
        if (options.Users.Count == 0) failures.Add("HomologationAuth requires at least one explicitly provisioned user.");
        if (options.Users.Select(x => x.Username.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Users.Count
            || options.Users.Select(x => x.UserId).Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Users.Count)
            failures.Add("HomologationAuth users must have unique usernames and user IDs.");
        foreach (var user in options.Users)
        {
            // Report field names only; configuration values may contain secrets or personal data.
            if (string.IsNullOrWhiteSpace(user.Username) || user.Username != user.Username.Trim()
                || !IsId(user.UserId) || !IsId(user.TenantId) || !IsId(user.BrokerageId) || !IsId(user.BranchId)
                || user.BranchIds.Count == 0 || user.BranchIds.Any(x => !IsId(x))
                || !user.BranchIds.Contains(user.BranchId, StringComparer.OrdinalIgnoreCase)
                || (user.SellerId is not null && !IsId(user.SellerId)))
                failures.Add("HomologationAuth requires username and valid user, tenant, brokerage and branch IDs, with the active branch in BranchIds.");
            if (user.UserType != UserTypes.BrokerageStaff || user.Roles.Count == 0 || user.Roles.Any(x => !AllowedRoles.Contains(x)))
                failures.Add("HomologationAuth only supports explicitly scoped brokerage staff roles.");
            if (!IsStrongIdentityHash(user.PasswordHash))
                failures.Add("HomologationAuth requires an ASP.NET Identity V3 password hash (SHA512, 100000+ iterations, 16-byte salt, 32-byte subkey).");
        }
        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool IsId(string value) => Guid.TryParse(value, out var id) && id != Guid.Empty;

    internal static bool IsStrongIdentityHash(string hash)
    {
        if (hash.Length > 512) return false;
        try
        {
            var bytes = Convert.FromBase64String(hash);
            if (bytes.Length < 61 || bytes[0] != 1) return false;
            var prf = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(1, 4));
            var iterations = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(5, 4));
            var saltLength = BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(9, 4));
            return prf == 2 && iterations is >= 100_000 and <= 1_000_000
                && saltLength >= 16 && saltLength <= bytes.Length - 45;
        }
        catch (FormatException) { return false; }
    }
}
