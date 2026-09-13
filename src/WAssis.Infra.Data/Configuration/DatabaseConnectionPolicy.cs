using Npgsql;

namespace WAssis.Infra.Data.Configuration;

public static class DatabaseConnectionPolicy
{
    public static void ValidateSharedEnvironment(string connectionString)
    {
        NpgsqlConnectionStringBuilder connection;
        try { connection = new NpgsqlConnectionStringBuilder(connectionString); }
        catch (ArgumentException) { throw new InvalidOperationException("Database connection configuration is invalid."); }
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.Database)
            || string.IsNullOrWhiteSpace(connection.Username) || connection.Host.Contains("...", StringComparison.Ordinal)
            || connection.Password is not { Length: >= 20 } password || password.Distinct().Count() < 10
            || password.Contains("configure", StringComparison.OrdinalIgnoreCase)
            || password.Contains("placeholder", StringComparison.OrdinalIgnoreCase)
            || connection.IncludeErrorDetail || connection.LogParameters)
            throw new InvalidOperationException("Shared environments require explicit database settings, a strong password, and sensitive database diagnostics disabled.");
    }
}
