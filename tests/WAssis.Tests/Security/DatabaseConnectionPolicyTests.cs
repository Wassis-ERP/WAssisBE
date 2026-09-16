using System.Security.Cryptography;
using Npgsql;
using WAssis.Infra.Data.Configuration;

namespace WAssis.Tests.Security;

public sealed class DatabaseConnectionPolicyTests
{
    [Theory]
    [InlineData("")]
    [InlineData("postgres")]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("configure-a-strong-password-placeholder")]
    public void RejectsMissingWeakOrPlaceholderPasswords(string password)
    {
        var connection = new NpgsqlConnectionStringBuilder { Host = "localhost", Database = "fixture", Username = "fixture", Password = password };
        var error = Assert.Throws<InvalidOperationException>(() => DatabaseConnectionPolicy.ValidateSharedEnvironment(connection.ConnectionString));
        Assert.DoesNotContain("Password=", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AcceptsStrongConfigurationAndRejectsSensitiveDiagnostics()
    {
        var connection = new NpgsqlConnectionStringBuilder
        {
            Host = "db.example.test",
            Database = "fixture",
            Username = "fixture",
            Password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            SslMode = SslMode.VerifyFull,
        };
        DatabaseConnectionPolicy.ValidateSharedEnvironment(connection.ConnectionString);
        connection.IncludeErrorDetail = true;
        Assert.Throws<InvalidOperationException>(() => DatabaseConnectionPolicy.ValidateSharedEnvironment(connection.ConnectionString));
    }

    [Theory]
    [InlineData(SslMode.Disable)]
    [InlineData(SslMode.Require)]
    [InlineData(SslMode.Prefer)]
    public void RejectsConnectionsWithoutCertificateVerification(SslMode sslMode)
    {
        var connection = new NpgsqlConnectionStringBuilder
        {
            Host = "db.example.test",
            Database = "fixture",
            Username = "fixture",
            Password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            SslMode = sslMode,
        };
        Assert.Throws<InvalidOperationException>(() => DatabaseConnectionPolicy.ValidateSharedEnvironment(connection.ConnectionString));
    }
}
