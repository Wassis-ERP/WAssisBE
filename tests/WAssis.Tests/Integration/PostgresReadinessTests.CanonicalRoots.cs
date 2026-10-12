using Microsoft.EntityFrameworkCore;
using WAssis.Infra.Data.Context;

namespace WAssis.Tests.Integration;

public sealed partial class PostgresReadinessTests
{
    private static async Task VerifyCanonicalCalculationReferences(WAssisDbContext db, Guid opportunityId, Guid insuredId)
    {
        Assert.Equal(0, await db.Database.SqlQueryRaw<int>("""
            SELECT count(*)::int AS "Value" FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name IN ('segurados','oportunidades')
            """).SingleAsync());
        var branchOfInsurance = Guid.NewGuid();
        var calculation = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.ramos (id,tenant_id,nome,risk_type,forma_calculo,ativo)
            VALUES ({branchOfInsurance},{Guid.Parse(TenantA)},'Auto fixture','VEICULO','AUTO',true);
            INSERT INTO erp.calculos (id,oportunidade_id,ramo_id,segurado_id,origem,tipo_seguro)
            VALUES ({calculation},{opportunityId},{branchOfInsurance},{insuredId},'MANUAL','NOVO');
            INSERT INTO erp.calc_auto (calculo_id,condutor_nome) VALUES ({calculation},'Condutor da versão');
            """);
        Assert.Equal(1, await db.Database.SqlQuery<int>($"""
            SELECT count(*)::int AS "Value" FROM erp.calculos c
            JOIN erp.oportunidades o ON o.id = c.oportunidade_id
            JOIN erp.segurados s ON s.id = c.segurado_id
            WHERE c.id = {calculation} AND o.id = {opportunityId} AND s.id = {insuredId}
            """).SingleAsync());
    }
}
