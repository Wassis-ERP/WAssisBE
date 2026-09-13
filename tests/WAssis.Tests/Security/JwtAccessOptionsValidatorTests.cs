using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using WAssis.Infra.CrossCutting.Identity.Jwt;

namespace WAssis.Tests.Security;

public sealed class JwtAccessOptionsValidatorTests
{
    [Fact]
    public void Validate_ShouldFailInProduction_WhenUsingPlaceholderKeyOrHttpMetadataDisabled()
    {
        var validator = new JwtAccessOptionsValidator(new FakeHostEnvironment("Production"));
        var options = new JwtAccessOptions
        {
            Issuer = "WAssis",
            Audience = "WAssis.Clients",
            SigningKey = "__configure_in_user_secrets_or_env__",
            RequireHttpsMetadata = false
        };

        var result = validator.Validate(null, options);

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failures);
        Assert.Contains(result.Failures, static failure => failure.Contains("SigningKey", StringComparison.Ordinal));
        Assert.Contains(result.Failures, static failure => failure.Contains("RequireHttpsMetadata", StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_ShouldSucceedInProduction_WhenConfigurationIsHardened()
    {
        var validator = new JwtAccessOptionsValidator(new FakeHostEnvironment("Production"));
        var options = new JwtAccessOptions
        {
            Issuer = "WAssis",
            Audience = "WAssis.Clients",
            SigningKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48)),
            RequireHttpsMetadata = true
        };

        var result = validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData("")]
    [InlineData("configure-uma-chave-com-mais-de-32-caracteres")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public void Validate_RejectsMissingPlaceholderAndRepeatedKeys(string key)
    {
        var result = new JwtAccessOptionsValidator(new FakeHostEnvironment("Staging")).Validate(null,
            new JwtAccessOptions { SigningKey = key, RequireHttpsMetadata = true });
        Assert.False(result.Succeeded);
    }

    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "WAssis.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
