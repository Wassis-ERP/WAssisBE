using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using WAssis.Infra.CrossCutting.Identity.Authentication;

namespace WAssis.Tests.Security;

public sealed class Auth0OptionsValidatorTests
{
    [Fact]
    public void Production_RequiresAuth0Configuration()
    {
        var result = new Auth0OptionsValidator(new TestEnvironment("Production"))
            .Validate(null, new Auth0Options { RequiredInProduction = true });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public void Production_AcceptsHttpsBffConfigurationWithoutRepositorySecret()
    {
        var result = new Auth0OptionsValidator(new TestEnvironment("Production")).Validate(null, new Auth0Options
        {
            Enabled = true,
            Authority = "https://example.auth0.com/",
            ClientId = "fixture-client",
            ClientSecret = "provided-by-external-secret-store",
            FrontendCallbackUrl = "https://crm.example.invalid/auth/callback",
            FrontendLogoutUrl = "https://crm.example.invalid/login",
            AccessTokenMinutes = 10,
        });

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("http://example.auth0.com", "https://crm.example.invalid", "https://crm.example.invalid")]
    [InlineData("https://example.auth0.com", "http://crm.example.invalid", "https://crm.example.invalid")]
    [InlineData("https://example.auth0.com", "https://crm.example.invalid", "http://crm.example.invalid")]
    public void Production_RejectsNonTlsProviderOrRedirects(string authority, string callback, string logout)
    {
        var result = new Auth0OptionsValidator(new TestEnvironment("Production")).Validate(null, new Auth0Options
        {
            Enabled = true,
            Authority = authority,
            ClientId = "fixture-client",
            ClientSecret = "provided-by-external-secret-store",
            FrontendCallbackUrl = callback,
            FrontendLogoutUrl = logout,
        });

        Assert.False(result.Succeeded);
    }

    private sealed class TestEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "WAssis.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
