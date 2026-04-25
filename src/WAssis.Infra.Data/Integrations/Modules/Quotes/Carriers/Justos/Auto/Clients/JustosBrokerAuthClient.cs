using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WAssis.Infra.Data.Configuration;
using WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto.Models;

namespace WAssis.Infra.Data.Integrations.Modules.Quotes.Carriers.Justos.Auto.Clients;

public sealed class JustosBrokerAuthClient(HttpClient httpClient, IOptions<JustosAutoQuoteOptions> optionsAccessor)
{
    private readonly JustosAutoQuoteOptions _options = optionsAccessor.Value;

    public bool HasValidConfiguration =>
        !string.IsNullOrWhiteSpace(_options.Issuer) &&
        !string.IsNullOrWhiteSpace(_options.BrokerId) &&
        !string.IsNullOrWhiteSpace(_options.PrivateKeyPemPath);

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (!HasValidConfiguration || !File.Exists(_options.PrivateKeyPemPath))
        {
            return null;
        }

        if (!int.TryParse(_options.BrokerId, out var brokerId))
        {
            return null;
        }

        var jwt = BuildPartnerJwt();
        var response = await httpClient.PostAsJsonAsync(
            "/brokers/auth/api-token",
            new JustosApiTokenRequest(jwt, brokerId),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<JustosApiTokenResponse>(cancellationToken: cancellationToken);
        return payload?.Token;
    }

    private string BuildPartnerJwt()
    {
        var privateKeyPem = File.ReadAllText(_options.PrivateKeyPemPath!);
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privateKeyPem);

        var credentials = new SigningCredentials(new ECDsaSecurityKey(ecdsa), SecurityAlgorithms.EcdsaSha256);
        var now = DateTimeOffset.UtcNow;
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: "justos",
            claims: null,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(10).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
