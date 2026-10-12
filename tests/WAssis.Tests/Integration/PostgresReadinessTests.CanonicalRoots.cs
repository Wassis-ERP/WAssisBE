using Microsoft.EntityFrameworkCore;
using Npgsql;
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

        // Database enforcement also protects writes that bypass API validation.
        var otherBranchInsured = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO erp.segurados (id,tenant_id,filial_id,nome,tipo,status,lgpd_autorizado,created_at,updated_at)
            VALUES ({otherBranchInsured},{Guid.Parse(TenantA)},{Guid.Parse(BranchB)},'Outra corretora','PF','Prospecto',false,now(),now())
            """);
        var conflict = await Assert.ThrowsAsync<PostgresException>(() => db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE erp.oportunidades SET segurado_id = {otherBranchInsured} WHERE id = {opportunityId}
            """));
        Assert.Equal(PostgresErrorCodes.CheckViolation, conflict.SqlState);
        Assert.Equal(insuredId, await db.Database.SqlQuery<Guid>($"""
            SELECT segurado_id AS "Value" FROM erp.oportunidades WHERE id = {opportunityId}
            """).SingleAsync());
    }
}
