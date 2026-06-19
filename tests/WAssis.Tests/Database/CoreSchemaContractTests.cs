using System.Reflection;
using WAssis.Infra.Data.Context;

namespace WAssis.Tests.Database;

public sealed class CoreSchemaContractTests
{
    private static readonly string[] ExpectedTables =
    [
        "tenants", "filiais", "profiles", "profile_filiais", "role_permissions",
        "produtores", "segurados", "pessoa_contato", "seguradoras", "ramos", "origens", "motivos_perda", "coberturas_catalogo",
        "pipelines", "pipeline_stages", "oportunidades", "calculos", "calc_auto", "calc_residencia", "calc_condominio", "calc_vida",
        "calc_empresa", "calc_diversos", "calculo_coberturas", "cotacoes", "apolices", "propostas", "apolice_itens", "item_veiculo",
        "item_imovel", "item_empresa", "item_vida", "item_coberturas", "sinistros", "sinistro_envolvidos", "pos_vendas",
        "recebimento_grades", "recebimento_grade_parcelas", "repasse_regras", "parcelas", "financeiro_cobrancas", "comissoes", "repasses",
        "atividades", "atividade_mencoes", "anexos", "audit_logs", "integracao_logs", "campo_definicoes", "campo_opcoes", "campo_valores",
        "campo_valor_opcoes",
    ];

    [Fact]
    public void EmbeddedMigrationSql_ShouldCreateEveryDbmlTableInErpSchema()
    {
        var sql = ReadMigrationSql();

        foreach (var table in ExpectedTables)
        {
            Assert.Contains($"CREATE TABLE erp.\"{table}\"", sql, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void EmbeddedMigrationSql_ShouldInstallTenantBranchGuards()
    {
        var sql = ReadMigrationSql();

        Assert.Contains("enforce_filial_tenant_match", sql, StringComparison.Ordinal);
        Assert.Contains("enforce_profile_filial_tenant_match", sql, StringComparison.Ordinal);
        Assert.Contains("ux_profile_filiais_profile_filial", sql, StringComparison.Ordinal);
        Assert.Contains("VALUES ('1.0.0'", sql, StringComparison.Ordinal);
    }

    private static string ReadMigrationSql()
    {
        var assembly = typeof(WAssisDbContext).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(name =>
            name.EndsWith("Migrations.Sql.20260619213000_CreateErpCoreV1Schema.sql", StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
