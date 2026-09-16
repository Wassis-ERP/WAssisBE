using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dapper;
using DotNet.Testcontainers.Configurations;
using Npgsql;
using Testcontainers.PostgreSql;

namespace WAssis.Tests.Integration;

/// <summary>A unique test database, using verified TLS in Docker or an explicitly supplied disposable local cluster.</summary>
internal sealed class PostgresTestDatabase : IAsyncDisposable
{
    private readonly PostgreSqlContainer? _container;
    private readonly string? _certificatePath;
    private readonly string _adminConnection;
    private readonly string _database = "wassis_test_" + Guid.NewGuid().ToString("N");
    private bool _created;
    private string? _applicationRole;

    private PostgresTestDatabase(string adminConnection, PostgreSqlContainer? container, string? certificatePath)
    {
        _adminConnection = adminConnection;
        _container = container;
        _certificatePath = certificatePath;
    }

    public string GetConnectionString() => new NpgsqlConnectionStringBuilder(_adminConnection) { Database = _database }.ConnectionString;

    public async Task<string> CreateApplicationConnectionAsync()
    {
        if (_applicationRole is not null) throw new InvalidOperationException("Application role already created.");
        var role = "wassis_app_" + Guid.NewGuid().ToString("N");
        var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await using var admin = new NpgsqlConnection(GetConnectionString());
        await admin.OpenAsync();
        // Identifiers and password are generated hexadecimal values, never supplied by callers.
        await admin.ExecuteAsync($"CREATE ROLE {role} LOGIN PASSWORD '{password}' NOSUPERUSER NOCREATEDB NOCREATEROLE NOINHERIT NOBYPASSRLS");
        _applicationRole = role;
        foreach (var schema in new[] { "public", "erp", "quotes", "documents", "financial", "policies", "billing", "whatsapp_support", "operations", "infrastructure", "security" })
        {
            await admin.ExecuteAsync($"""
                GRANT USAGE ON SCHEMA {schema} TO {role};
                GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA {schema} TO {role};
                GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA {schema} TO {role};
                """);
        }
        var settings = new NpgsqlConnectionStringBuilder(GetConnectionString()) { Username = role, Password = password };
        await using var application = new NpgsqlConnection(settings.ConnectionString);
        await application.OpenAsync();
        var bypass = await application.ExecuteScalarAsync<bool>("SELECT rolsuper OR rolbypassrls FROM pg_roles WHERE rolname = current_user");
        if (bypass) throw new InvalidOperationException("Application fixture must enforce RLS.");
        return settings.ConnectionString;
    }

    public static async Task<PostgresTestDatabase> StartAsync()
    {
        var local = Environment.GetEnvironmentVariable("WASSIS_TEST_POSTGRES_CONNECTION");
        PostgresTestDatabase fixture;
        if (local is not null)
        {
            var settings = new NpgsqlConnectionStringBuilder(local);
            if (settings.Host is not ("localhost" or "127.0.0.1") || settings.Database != "postgres"
                || settings.SslMode != SslMode.VerifyFull || string.IsNullOrWhiteSpace(settings.RootCertificate))
                throw new InvalidOperationException("Use a disposable loopback cluster, database postgres and VerifyFull with a test CA.");
            fixture = new(local, null, null);
        }
        else
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyCertSign, true));
            var names = new SubjectAlternativeNameBuilder();
            names.AddDnsName("localhost"); names.AddIpAddress(IPAddress.Loopback);
            request.CertificateExtensions.Add(names.Build());
            using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(1));
            var certificatePath = Path.Combine(Path.GetTempPath(), "wassis-test-ca-" + Guid.NewGuid().ToString("N") + ".pem");
            await File.WriteAllTextAsync(certificatePath, certificate.ExportCertificatePem());
            var postgres = new PostgreSqlBuilder("postgres:16-bookworm")
                .WithPassword(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)))
                .WithResourceMapping(Encoding.UTF8.GetBytes(certificate.ExportCertificatePem()), "/tmp/server.crt", 999, 999, UnixFileModes.UserRead)
                .WithResourceMapping(Encoding.UTF8.GetBytes(rsa.ExportPkcs8PrivateKeyPem()), "/tmp/server.key", 999, 999, UnixFileModes.UserRead)
                // PostgreSQLBuilder already runs the image entrypoint with `postgres`.
                // Supplying it again makes the server receive an invalid positional argument.
                .WithCommand("-c", "ssl=on", "-c", "ssl_cert_file=/tmp/server.crt", "-c", "ssl_key_file=/tmp/server.key")
                .Build();
            try { await postgres.StartAsync(); }
            catch { await postgres.DisposeAsync(); File.Delete(certificatePath); throw; }
            var settings = new NpgsqlConnectionStringBuilder(postgres.GetConnectionString())
            { Database = "postgres", SslMode = SslMode.VerifyFull, RootCertificate = certificatePath };
            fixture = new(settings.ConnectionString, postgres, certificatePath);
        }
        try
        {
            await using var admin = new NpgsqlConnection(fixture._adminConnection);
            await admin.OpenAsync();
            await admin.ExecuteAsync($"CREATE DATABASE {fixture._database}");
            fixture._created = true;
            return fixture;
        }
        catch { await fixture.DisposeAsync(); throw; }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_created)
            {
                await using var admin = new NpgsqlConnection(_adminConnection);
                await admin.OpenAsync();
                // Only the name generated by this fixture is ever deleted.
                await admin.ExecuteAsync($"DROP DATABASE {_database} WITH (FORCE)");
                if (_applicationRole is not null) await admin.ExecuteAsync($"DROP ROLE {_applicationRole}");
            }
        }
        finally
        {
            if (_container is not null) await _container.DisposeAsync();
            if (_certificatePath is not null) File.Delete(_certificatePath);
        }
    }
}
