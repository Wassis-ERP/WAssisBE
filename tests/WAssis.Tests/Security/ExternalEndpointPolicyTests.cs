using WAssis.Infra.Data.Configuration;

namespace WAssis.Tests.Security;

public sealed class ExternalEndpointPolicyTests
{
    [Fact]
    public void AcceptsExactAllowlistedHttpsHost()
    {
        var result = ExternalEndpointPolicy.RequireHttpsAllowlisted(
            "https://api.example.test/v1", ["api.example.test"]);
        Assert.Equal("api.example.test", result.Host);
    }

    [Theory]
    [InlineData("http://api.example.test/v1")]
    [InlineData("https://user:password@api.example.test/v1")]
    [InlineData("https://127.0.0.1/v1")]
    [InlineData("https://evil.example.test/v1")]
    public void RejectsUnsafeOrNonAllowlistedDestination(string value)
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExternalEndpointPolicy.RequireHttpsAllowlisted(value, ["api.example.test"]));
    }
}
