using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using WAssis.Infra.Data.Context;

#nullable disable

namespace WAssis.Infra.Data.Migrations;

[DbContext(typeof(WAssisDbContext))]
[Migration("20260619213000_CreateErpCoreV1Schema")]
public sealed class CreateErpCoreV1Schema : Migration
{
    private const string ResourceSuffix = "Migrations.Sql.20260619213000_CreateErpCoreV1Schema.sql";

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(ReadEmbeddedSql());
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP SCHEMA IF EXISTS erp CASCADE;");
    }

    internal static string ReadEmbeddedSql()
    {
        var assembly = typeof(CreateErpCoreV1Schema).Assembly;
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(ResourceSuffix, StringComparison.Ordinal));

        if (resourceName is null)
        {
            throw new InvalidOperationException($"Embedded migration SQL ending with '{ResourceSuffix}' was not found.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded migration SQL '{resourceName}' could not be opened.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
