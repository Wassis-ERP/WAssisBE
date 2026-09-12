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

    private static readonly string[] ExpectedV31Tables =
    [
        "tenants", "filiais", "profiles", "perfis", "profile_filiais", "role_permissions",
        "produtores", "segurados", "pessoa_contato", "seguradoras", "ramos", "endosso_subtipos", "cancelamento_motivos",
        "origens", "motivos_perda", "coberturas_catalogo", "pipelines", "pipeline_stages", "oportunidades", "calculos",
        "calc_auto", "calc_residencia", "calc_condominio", "calc_vida", "calc_empresa", "calc_diversos", "calculo_coberturas",
        "calculo_execucoes", "cotacoes", "cotacao_coberturas", "cotacao_parcelamentos", "apresentacoes_comerciais",
        "apresentacao_cotacoes", "apolices", "propostas", "apolice_itens", "item_veiculo", "item_imovel", "item_empresa",
        "item_vida", "item_coberturas", "sinistros", "sinistro_envolvidos", "pos_vendas", "recebimento_grades",
        "recebimento_grade_parcelas", "repasse_regras", "parcelas", "financeiro_cobrancas", "comissoes", "comissao_extratos",
        "comissao_extrato_itens", "comissao_conciliacoes", "comissao_conciliacao_ocorrencias", "comissao_baixas",
        "comissao_baixa_conciliacoes", "repasses", "repasse_recibos", "repasse_recibo_itens", "atividades",
        "atividade_mencoes", "anexos", "audit_logs", "integracao_logs", "campo_definicoes", "campo_opcoes",
        "campo_valores", "campo_valor_opcoes",
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
        Assert.Empty(FindDuplicateIndexNames(sql));
    }

    [Fact]
    public void FrontendContractMigration_ShouldExpandCatalogsAndMultiCalculationWithoutBusinessJson()
    {
        var assembly = typeof(WAssisDbContext).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(name =>
            name.EndsWith("Migrations.Sql.20260825000800_ExpandErpFrontendContract.sql", StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        Assert.Contains("CREATE TABLE erp.perfis", sql, StringComparison.Ordinal);
        Assert.Contains("ALTER TABLE erp.calculos", sql, StringComparison.Ordinal);
        Assert.Contains("ALTER TABLE erp.calc_auto", sql, StringComparison.Ordinal);
        Assert.Contains("ALTER TABLE erp.recebimento_grades", sql, StringComparison.Ordinal);
        Assert.Contains("enforce_profile_filial_perfil_tenant", sql, StringComparison.Ordinal);
        Assert.Contains("enforce_calculo_scope", sql, StringComparison.Ordinal);
        Assert.Contains("enforce_repasse_regra_scope", sql, StringComparison.Ordinal);
        Assert.Contains("VALUES ('1.1.0'", sql, StringComparison.Ordinal);
        Assert.DoesNotContain("ADD COLUMN dados jsonb", sql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void V31ContractMigration_ShouldAddEveryCanonicalTableAndCriticalConstraint()
    {
        var assembly = typeof(WAssisDbContext).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(name =>
            name.EndsWith("Migrations.Sql.20260912120000_AlignErpContractV31.sql", StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var sql = reader.ReadToEnd();

        Assert.Equal(68, ExpectedV31Tables.Length);
        foreach (var table in ExpectedV31Tables)
        {
            Assert.Contains($"CREATE TABLE IF NOT EXISTS erp.\"{table}\"", sql, StringComparison.Ordinal);
        }

        Assert.Contains("ADD COLUMN IF NOT EXISTS \"nome_social\" text", sql, StringComparison.Ordinal);
        Assert.Contains("ADD COLUMN IF NOT EXISTS \"apolice_origem_id\" uuid", sql, StringComparison.Ordinal);
        Assert.Contains("ux_calculo_execucoes_calculo_id_seguradora_id_tentativa", sql, StringComparison.Ordinal);
        Assert.Contains("ux_cotacoes_execucao_id", sql, StringComparison.Ordinal);
        Assert.Contains("ux_apresentacao_cotacoes_apresentacao_id_ordem", sql, StringComparison.Ordinal);
        Assert.Contains("VALUES ('3.1.0'", sql, StringComparison.Ordinal);
        Assert.DoesNotContain(" jsonb", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(FindDuplicateIndexNames(sql));
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

    private static IReadOnlyCollection<string> FindDuplicateIndexNames(string sql)
    {
        return System.Text.RegularExpressions.Regex
            .Matches(sql, "CREATE(?: UNIQUE)? INDEX(?: IF NOT EXISTS)? \\\"?([^\\\"\\s]+)\\\"?", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
            .Select(static match => match.Groups[1].Value)
            .GroupBy(static name => name, StringComparer.OrdinalIgnoreCase)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();
    }
}
