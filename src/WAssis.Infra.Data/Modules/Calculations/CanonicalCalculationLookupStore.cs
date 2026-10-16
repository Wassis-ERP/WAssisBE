using System.Globalization;
using Dapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Calculations;
using WAssis.Domain.Modules.Opportunities.Entities;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Calculations;

public sealed class CanonicalCalculationLookupStore(WAssisDbContext db, ICurrentUserContext user, TimeProvider clock) : ICanonicalCalculationLookupStore
{
    private CommandDefinition Command(string sql, object? parameters, CancellationToken token) =>
        new(sql, parameters, db.Database.CurrentTransaction?.GetDbTransaction(), commandTimeout: 30, cancellationToken: token);
    private Task<Opportunity?> OpportunityAsync(Guid id, CancellationToken token) => !user.IsAuthenticated
        || !Guid.TryParse(user.TenantId, out _) || !Guid.TryParse(user.BranchId, out _) ? Task.FromResult<Opportunity?>(null)
        : db.Opportunities.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.OfficeBranchId == user.BranchId, token);

    public async Task<PagedResult<CalculationInsurerOption>?> ListInsurersAsync(Guid opportunityId, int page, int pageSize, string? search, CancellationToken token)
    {
        await new ListCalculationInsurersQueryValidator().ValidateAndThrowAsync(new(opportunityId, page, pageSize, search), token);
        var opportunity = await OpportunityAsync(opportunityId, token);
        if (opportunity is null) return null;
        await db.Database.OpenConnectionAsync(token);
        var parameters = new { Tenant = Guid.Parse(opportunity.TenantId), Search = search, Limit = pageSize, Offset = (long)(page - 1) * pageSize };
        const string source = "FROM erp.seguradoras s WHERE s.tenant_id=@Tenant AND s.ativo AND (@Search IS NULL OR COALESCE(s.nome,'') ILIKE '%'||@Search||'%')";
        var connection = db.Database.GetDbConnection();
        var count = await connection.ExecuteScalarAsync<long>(Command("SELECT count(*) " + source, parameters, token));
        var rows = await connection.QueryAsync<CalculationInsurerOption>(Command("""
            SELECT s.id AS "Id",s.tenant_id AS "TenantId",COALESCE(s.nome,'Seguradora sem nome') AS "Name"
            """ + " " + source + " ORDER BY s.nome NULLS LAST,s.id LIMIT @Limit OFFSET @Offset", parameters, token));
        return new(rows.AsList(), page, pageSize, count);
    }
    public async Task<PagedResult<CalculationCoverageOption>?> ListCoveragesAsync(Guid opportunityId, Guid insuranceBranchId, int page, int pageSize, string? search, CancellationToken token)
    {
        await new ListCalculationCoveragesQueryValidator().ValidateAndThrowAsync(new(opportunityId, insuranceBranchId, page, pageSize, search), token);
        var opportunity = await OpportunityAsync(opportunityId, token);
        if (opportunity is null) return null;
        await db.Database.OpenConnectionAsync(token);
        if (!await ValidAutoBranchAsync(insuranceBranchId, Guid.Parse(opportunity.TenantId), token))
            throw new ValidationException("Ramo Auto indisponível.");
        var parameters = new { Line = insuranceBranchId, Search = search, Limit = pageSize, Offset = (long)(page - 1) * pageSize };
        const string source = "FROM erp.coberturas_catalogo c WHERE c.ramo_id=@Line AND c.ativo AND (@Search IS NULL OR COALESCE(c.nome,c.codigo,'') ILIKE '%'||@Search||'%')";
        var connection = db.Database.GetDbConnection();
        var count = await connection.ExecuteScalarAsync<long>(Command("SELECT count(*) " + source, parameters, token));
        var rows = await connection.QueryAsync<CalculationCoverageOption>(Command(
            "SELECT " + CanonicalCalculationSql.Select<CalculationCoverageOption>("c") + " " + source + " ORDER BY c.ordem NULLS LAST,c.nome NULLS LAST,c.id LIMIT @Limit OFFSET @Offset", parameters, token));
        return new(rows.AsList(), page, pageSize, count);
    }
    public async Task<CalculationEditorContext?> GetContextAsync(Guid opportunityId, Guid? insuranceBranchId, CancellationToken token)
    {
        await new GetCalculationContextQueryValidator().ValidateAndThrowAsync(new(opportunityId, insuranceBranchId), token);
        var opportunity = await OpportunityAsync(opportunityId, token);
        if (opportunity is null) return null;
        await db.Database.OpenConnectionAsync(token);
        var tenant = Guid.Parse(opportunity.TenantId); var branch = Guid.Parse(user.BranchId!);
        var line = insuranceBranchId ?? (Guid.TryParse(opportunity.InsuranceLineId, out var linkedLine) ? linkedLine : (Guid?)null);
        if (line.HasValue && !await ValidAutoBranchAsync(line.Value, tenant, token)) throw new ValidationException("Ramo Auto indisponível.");
        var zone = await db.Database.GetDbConnection().QuerySingleOrDefaultAsync<string>(Command("""
            SELECT t.timezone FROM erp.tenants t JOIN erp.filiais f ON f.tenant_id=t.id WHERE t.id=@Tenant AND f.id=@Branch AND f.ativo
            """, new { Tenant = tenant, Branch = branch }, token));
        if (zone is null || !TimeZoneInfo.TryFindSystemTimeZoneById(zone, out var timeZone)) throw new ValidationException("Configure o fuso horário do grupo para a corretora.");
        CalculationSourcePolicy? source = null;
        if (opportunity.OriginPolicyId.HasValue && line.HasValue) source = await db.Database.GetDbConnection().QuerySingleOrDefaultAsync<CalculationSourcePolicy>(Command("""
            SELECT a.id AS "Id",a.ramo_id AS "InsuranceBranchId",a.segurado_id AS "InsuredPersonId",a.seguradora_id AS "InsurerId",
              a.numero_apolice AS "Number",a.vigencia_fim::text AS "CoverageEndsOn",a.status AS "Status"
            FROM erp.apolices a JOIN erp.segurados s ON s.id=a.segurado_id
            WHERE a.id=@Id AND a.ramo_id=@Line AND s.tenant_id=@Tenant AND s.filial_id=@Branch
            """, new { Id = opportunity.OriginPolicyId, Line = line, Tenant = tenant, Branch = branch }, token));
        var date = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), timeZone).DateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return new(opportunityId, line, opportunity.InsuredPersonId, opportunity.LeadName, branch, zone, date, source);
    }
    private Task<bool> ValidAutoBranchAsync(Guid line, Guid tenant, CancellationToken token) => db.Database.GetDbConnection().ExecuteScalarAsync<bool>(Command(
        "SELECT EXISTS(SELECT 1 FROM erp.ramos WHERE id=@Line AND tenant_id=@Tenant AND ativo AND forma_calculo='AUTO')", new { Line = line, Tenant = tenant }, token));
}
