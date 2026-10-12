using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Calculations;

public sealed class CanonicalCalculationStore(WAssisDbContext db, ICurrentUserContext user, TimeProvider clock) : ICanonicalCalculationStore
{
    private CommandDefinition Command(string sql, object? parameters, CancellationToken token) =>
        new(sql, parameters, db.Database.CurrentTransaction?.GetDbTransaction(), commandTimeout: 30, cancellationToken: token);

    public async Task<CalculationAggregate?> CreateAsync(Guid opportunityId, string key, CreateAutoCalculation input, CancellationToken cancellationToken)
    {
        await new CreateCalculationCommandValidator().ValidateAndThrowAsync(new(opportunityId, key, input), cancellationToken);
        await using var transaction = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(cancellationToken) : null;
        await db.Database.OpenConnectionAsync(cancellationToken);
        if (!user.IsAuthenticated || !Guid.TryParse(user.TenantId, out var tenant) || !Guid.TryParse(user.BranchId, out var branch))
            throw new UnauthorizedAccessException("Contexto de corretora obrigatório.");
        var connection = db.Database.GetDbConnection();
        var lockKey = $"calculations:{tenant}:{branch}:{opportunityId}:{key}";
        await connection.ExecuteAsync(Command("SELECT pg_advisory_xact_lock(hashtextextended(@Key,0))", new { Key = lockKey }, cancellationToken));
        // Lock first; re-evaluate ownership afterwards to close the reassignment race.
        var locked = await connection.QuerySingleOrDefaultAsync<Guid?>(Command("""
            SELECT id FROM erp.oportunidades WHERE id=@Id AND tenant_id=@Tenant AND filial_id=@Branch FOR UPDATE
            """, new { Id = opportunityId, Tenant = tenant, Branch = branch }, cancellationToken));
        if (!locked.HasValue) return null;
        var opportunity = await db.Opportunities.AsNoTracking().SingleOrDefaultAsync(x => x.Id == opportunityId, cancellationToken);
        if (opportunity is null) return null;

        var normalized = input with
        {
            OpportunityId = opportunityId,
            Specialization = input.Specialization with { Data = input.Specialization.Data with { CalculationId = Guid.Empty } },
            Coverages = input.Coverages.OrderBy(c => c.CoverageId).ToArray()
        };
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(normalized))));
        var receipt = await connection.QuerySingleOrDefaultAsync<Receipt>(Command("""
            SELECT calculation_id AS "CalculationId", request_hash AS "RequestHash" FROM infrastructure.calculation_receipts
            WHERE tenant_id=@Tenant AND branch_id=@Branch AND opportunity_id=@Opportunity AND idempotency_key=@Key
            """, new { Tenant = tenant, Branch = branch, Opportunity = opportunityId, Key = key }, cancellationToken));
        if (receipt is not null)
        {
            if (receipt.RequestHash != hash) throw new CalculationConflictException();
            var existing = await GetAsync(receipt.CalculationId, cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return existing;
        }

        var validLine = await connection.ExecuteScalarAsync<bool>(Command("""
            SELECT EXISTS(SELECT 1 FROM erp.ramos WHERE id=@Id AND tenant_id=@Tenant AND ativo AND forma_calculo='AUTO')
            """, new { Id = input.InsuranceBranchId, Tenant = tenant }, cancellationToken));
        if (!validLine) throw new ValidationException("Ramo Auto indisponível.");
        var insuredId = input.InsuredPersonId ?? opportunity.InsuredPersonId;
        string? insuredName = opportunity.LeadName;
        if (insuredId.HasValue)
        {
            var insured = await db.InsuredPeople.AsNoTracking().SingleOrDefaultAsync(x => x.Id == insuredId && x.OfficeBranchId == user.BranchId, cancellationToken);
            if (insured is null) throw new UnauthorizedAccessException("Cotado indisponível nesta corretora.");
            insuredName = insured.Name;
        }
        var catalogIds = input.Coverages.Select(c => c.CoverageId).ToArray();
        var validCoverageCount = await connection.ExecuteScalarAsync<int>(Command("""
            SELECT count(*)::int FROM erp.coberturas_catalogo WHERE id=ANY(@Ids) AND ramo_id=@Line AND ativo
            """, new { Ids = catalogIds, Line = input.InsuranceBranchId }, cancellationToken));
        if (validCoverageCount != catalogIds.Length) throw new ValidationException("Coberturas indisponíveis neste ramo.");

        var fields = await ResolveTermAsync(normalized, opportunity.OriginPolicyId, tenant, branch, cancellationToken);
        if (fields.PreviousInsurerId.HasValue && !await connection.ExecuteScalarAsync<bool>(Command("""
            SELECT EXISTS(SELECT 1 FROM erp.seguradoras WHERE id=@Id AND tenant_id=@Tenant AND ativo)
            """, new { Id = fields.PreviousInsurerId, Tenant = tenant }, cancellationToken)))
            throw new ValidationException("Seguradora anterior indisponível.");
        var id = Guid.NewGuid();
        var header = new CalculationRow
        {
            Id = id, OpportunityId = opportunityId, InsuranceBranchId = fields.InsuranceBranchId,
            InsuredPersonId = insuredId, PreviousInsurerId = fields.PreviousInsurerId, Origin = fields.Origin,
            SuggestedCommission = fields.SuggestedCommission, InsuranceType = fields.InsuranceType,
            Bonus = fields.Bonus, ClaimCount = fields.ClaimCount, PartialLossClaimCount = fields.PartialLossClaimCount,
            OwnershipTransferred = fields.OwnershipTransferred, CoverageStartsOn = fields.CoverageStartsOn,
            CoverageEndsOn = fields.CoverageEndsOn, PreviousCoverageEndsOn = fields.PreviousCoverageEndsOn,
            PreviousPolicyNumber = fields.PreviousPolicyNumber, PreviousIdentification = fields.PreviousIdentification,
            PreviousPolicyStatus = fields.PreviousPolicyStatus, InternalNote = fields.InternalNote,
            CreatedAtUtc = clock.GetUtcNow().UtcDateTime,
            VersionLabel = BuildLabel(insuredName, input.Specialization.Data, fields, input.Coverages)
        };
        var headerInsert = CanonicalCalculationSql.Insert("calculos", header);
        await connection.ExecuteAsync(Command(headerInsert.Sql, headerInsert.Parameters, cancellationToken));
        var risk = input.Specialization.Data with { CalculationId = id };
        var riskInsert = CanonicalCalculationSql.Insert("calc_auto", risk);
        await connection.ExecuteAsync(Command(riskInsert.Sql, riskInsert.Parameters, cancellationToken));
        foreach (var coverage in input.Coverages)
        {
            var row = new CalculationCoverageRow
            {
                Id = Guid.NewGuid(), CalculationId = id, CoverageId = coverage.CoverageId,
                Selected = coverage.Selected, RequestedLimit = coverage.RequestedLimit,
                RequestedFipePercentage = coverage.RequestedFipePercentage, RequestedDeductibleType = coverage.RequestedDeductibleType,
                RequestedOption = coverage.RequestedOption, RequestedQuantity = coverage.RequestedQuantity,
                TransmittedObservation = coverage.TransmittedObservation
            };
            var insert = CanonicalCalculationSql.Insert("calculo_coberturas", row);
            await connection.ExecuteAsync(Command(insert.Sql, insert.Parameters, cancellationToken));
        }
        await connection.ExecuteAsync(Command("""
            INSERT INTO infrastructure.calculation_receipts (tenant_id,branch_id,opportunity_id,idempotency_key,request_hash,calculation_id)
            VALUES (@Tenant,@Branch,@Opportunity,@Key,@Hash,@Calculation)
            """, new { Tenant = tenant, Branch = branch, Opportunity = opportunityId, Key = key, Hash = hash, Calculation = id }, cancellationToken));
        var result = await GetAsync(id, cancellationToken);
        if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<CalculationAggregate?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        await db.Database.OpenConnectionAsync(cancellationToken);
        var connection = db.Database.GetDbConnection();
        var header = await connection.QuerySingleOrDefaultAsync<CalculationRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<CalculationRow>("c")} FROM erp.calculos c WHERE c.id=@Id", new { Id = id }, cancellationToken));
        if (header is null || !await db.Opportunities.AnyAsync(x => x.Id == header.OpportunityId, cancellationToken)) return null;
        var risk = await connection.QuerySingleOrDefaultAsync<AutoCalculationRisk>(Command(
            $"SELECT {CanonicalCalculationSql.Select<AutoCalculationRisk>("a")} FROM erp.calc_auto a WHERE a.calculo_id=@Id", new { Id = id }, cancellationToken));
        if (risk is null) return null;
        var coverages = await connection.QueryAsync<CalculationCoverageRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<CalculationCoverageRow>("c")} FROM erp.calculo_coberturas c WHERE c.calculo_id=@Id ORDER BY c.cobertura_id", new { Id = id }, cancellationToken));
        return new(header, new() { Data = risk }, coverages.AsList());
    }

    public async Task<PagedResult<CalculationAggregate>?> ListAsync(Guid opportunityId, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (!await db.Opportunities.AnyAsync(x => x.Id == opportunityId, cancellationToken)) return null;
        await db.Database.OpenConnectionAsync(cancellationToken);
        var parameters = new { Id = opportunityId, Offset = (long)(page - 1) * pageSize, Limit = pageSize };
        var count = await db.Database.GetDbConnection().ExecuteScalarAsync<long>(Command("SELECT count(*) FROM erp.calculos c JOIN erp.calc_auto a ON a.calculo_id=c.id WHERE c.oportunidade_id=@Id", parameters, cancellationToken));
        var ids = await db.Database.GetDbConnection().QueryAsync<Guid>(Command("""
            SELECT c.id FROM erp.calculos c JOIN erp.calc_auto a ON a.calculo_id=c.id WHERE c.oportunidade_id=@Id
            ORDER BY c.criado_em DESC NULLS LAST,c.id LIMIT @Limit OFFSET @Offset
            """, parameters, cancellationToken));
        var items = new List<CalculationAggregate>();
        foreach (var id in ids) { var item = await GetAsync(id, cancellationToken); if (item is not null) items.Add(item); }
        return new(items, page, pageSize, count);
    }

    private async Task<CalculationFields> ResolveTermAsync(CreateAutoCalculation input, Guid? sourcePolicy, Guid tenant, Guid branch, CancellationToken token)
    {
        var fields = (CalculationFields)input;
        string start;
        if (input.InsuranceType == "NOVO")
        {
            // The group timezone is the effective branch timezone in the v3.1 schema.
            var zone = await db.Database.GetDbConnection().QuerySingleOrDefaultAsync<string>(Command("""
                SELECT t.timezone FROM erp.filiais f JOIN erp.tenants t ON t.id=f.tenant_id
                WHERE f.id=@Branch AND f.tenant_id=@Tenant AND f.ativo
                """, new { Branch = branch, Tenant = tenant }, token));
            if (zone is null || !TimeZoneInfo.TryFindSystemTimeZoneById(zone, out var timeZone))
                throw new ValidationException("Configure o fuso horário do grupo para a corretora.");
            start = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), timeZone).DateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        else if (input.InsuranceType == "RENOVACAO_PROPRIA")
        {
            if (!sourcePolicy.HasValue) throw new ValidationException("A oportunidade exige apólice de origem.");
            var source = await db.Database.GetDbConnection().QuerySingleOrDefaultAsync<SourcePolicy>(Command("""
                SELECT a.vigencia_fim::text AS "EndsOn",a.numero_apolice AS "Number",a.seguradora_id AS "InsurerId",a.status AS "Status"
                FROM erp.apolices a JOIN erp.segurados s ON s.id=a.segurado_id
                WHERE a.id=@Id AND s.tenant_id=@Tenant AND s.filial_id=@Branch
                """, new { Id = sourcePolicy, Tenant = tenant, Branch = branch }, token));
            if (source is null || source.EndsOn is null || source.InsurerId is null || string.IsNullOrWhiteSpace(source.Number))
                throw new ValidationException("A apólice de origem não contém os dados necessários.");
            start = source.EndsOn;
            fields = input with { PreviousCoverageEndsOn = start, PreviousPolicyNumber = source.Number, PreviousInsurerId = source.InsurerId, PreviousPolicyStatus = source.Status };
        }
        else start = input.CoverageStartsOn!;
        var end = CalculationCalendar.EndOfAnnualTerm(start);
        if (input.CoverageStartsOn is not null && input.CoverageStartsOn != start || input.CoverageEndsOn is not null && input.CoverageEndsOn != end)
            throw new ValidationException("Vigência deve respeitar a regra de calendário do seguro.");
        return fields with { CoverageStartsOn = start, CoverageEndsOn = end };
    }

    private static string BuildLabel(string? name, AutoCalculationRisk risk, CalculationFields fields, IReadOnlyList<RequestedCalculationCoverage> coverages)
    {
        var limits = string.Join(" / ", coverages.Where(x => x.Selected == true).OrderBy(x => x.CoverageId)
            .Select(x => x.RequestedLimit?.ToString("0.##", CultureInfo.InvariantCulture) ?? x.RequestedFipePercentage?.ToString("0.##", CultureInfo.InvariantCulture) + "%"));
        return $"{name ?? "Lead"} · {risk.DriverName ?? "Condutor não informado"} · {risk.Model ?? risk.Plate ?? "Auto"} · {fields.InsuranceType} · {limits}";
    }
    private sealed record Receipt(Guid CalculationId, string RequestHash);
    private sealed record SourcePolicy(string? EndsOn, string? Number, Guid? InsurerId, string? Status);
}
