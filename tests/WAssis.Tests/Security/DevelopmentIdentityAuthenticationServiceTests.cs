using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WAssis.Infra.CrossCutting.Identity.Authentication;
using WAssis.Infra.CrossCutting.Identity.Jwt;

namespace WAssis.Tests.Security;

public sealed class DevelopmentIdentityAuthenticationServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_ShouldReturnToken_WhenDevelopmentUserMatches()
    {
        var service = CreateService("Development", enabled: true);

        var result = await service.AuthenticateAsync("broker.admin@wassis.local", "secret", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
        Assert.Equal("dev-broker-admin", result.Value.UserId);
        Assert.Equal("tenant-dev-broker", result.Value.TenantId);
        Assert.Contains("brokerage_admin", result.Value.Roles);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldRejectInvalidPassword()
    {
        var service = CreateService("Development", enabled: true);

        var result = await service.AuthenticateAsync("broker.admin@wassis.local", "wrong", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("identity.login.invalid_credentials", result.Error?.Code);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldBeDisabledOutsideDevelopmentByDefault()
    {
        var service = CreateService("Production", enabled: true, allowOutsideDevelopment: false);

        var result = await service.AuthenticateAsync("broker.admin@wassis.local", "secret", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("identity.login.disabled", result.Error?.Code);
    }

    private static DevelopmentIdentityAuthenticationService CreateService(
        string environmentName,
        bool enabled,
        bool allowOutsideDevelopment = false)
    {
        var developmentOptions = Options.Create(new DevelopmentAuthOptions
        {
            Enabled = enabled,
            AllowOutsideDevelopment = allowOutsideDevelopment,
            TokenExpirationMinutes = 30,
            Users =
            [
                new DevelopmentAuthUserOptions
                {
                    Username = "broker.admin@wassis.local",
                    Password = "secret",
                    UserId = "dev-broker-admin",
                    TenantId = "tenant-dev-broker",
                    BrokerageId = "brokerage-dev-001",
                    SellerId = "seller-admin-001",
                    UserType = "brokerage_staff",
                    Roles = ["brokerage_admin"]
                }
            ]
        });

        var jwtOptions = Options.Create(new JwtAccessOptions
        {
            Issuer = "WAssis",
            Audience = "WAssis.Clients",
            SigningKey = "test-signing-key-with-at-least-thirty-two-chars",
            RequireHttpsMetadata = false
        });

        return new DevelopmentIdentityAuthenticationService(
            developmentOptions,
            jwtOptions,
            new FakeHostEnvironment(environmentName));
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "WAssis.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
