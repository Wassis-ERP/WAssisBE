using System.Security.Cryptography;
using System.Text.Json;
using Dapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Calculations;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Calculations;

public sealed class CanonicalExecutionStore(WAssisDbContext db, ICurrentUserContext user, TimeProvider clock, IAuditTrailWriter audit) : ICanonicalExecutionStore
{
    private CommandDefinition Command(string sql, object? parameters, CancellationToken token) =>
        new(sql, parameters, db.Database.CurrentTransaction?.GetDbTransaction(), commandTimeout: 30, cancellationToken: token);

    public async Task<CalculationExecutionRow?> CreateAsync(Guid calculationId, string key, CreateCalculationExecution input, CancellationToken token)
    {
        await new CreateExecutionCommandValidator().ValidateAndThrowAsync(new(calculationId, key, input), token);
        return await MutateAsync(calculationId, true, "create", key, input, async (calculation, _) =>
        {
            if (await db.Database.GetDbConnection().ExecuteScalarAsync<bool>(Command(
                "SELECT EXISTS(SELECT 1 FROM erp.calculo_execucoes WHERE calculo_id=@Calculation AND seguradora_id=@Insurer)",
                new { Calculation = calculation.Id, Insurer = input.InsurerId }, token))) throw new ExecutionStateConflictException();
            var row = await InsertExecutionAsync(calculation, input.InsurerId, input.CommissionOverride, null, token);
            return new Mutation<CalculationExecutionRow>(row, row.Id, null);
        }, receipt => GetAsync(receipt.ExecutionId, token), token);
    }

    public async Task<CalculationExecutionRow?> RetryAsync(Guid executionId, string key, RetryCalculationExecution input, CancellationToken token)
    {
        await new RetryExecutionCommandValidator().ValidateAndThrowAsync(new(executionId, key, input), token);
        return await MutateAsync(executionId, false, "retry", key, input, async (calculation, previous) =>
        {
            if (previous!.Engine != "MANUAL" || IsOpen(previous.Status)) throw new ExecutionStateConflictException();
            var row = await InsertExecutionAsync(calculation, previous.InsurerId, input.CommissionOverride, previous.Id, token);
            return new Mutation<CalculationExecutionRow>(row, row.Id, null);
        }, receipt => GetAsync(receipt.ExecutionId, token), token);
    }

    public async Task<CalculationExecutionRow?> CancelAsync(Guid executionId, string key, CancellationToken token)
    {
        await new CancelExecutionCommandValidator().ValidateAndThrowAsync(new(executionId, key), token);
        return await MutateAsync(executionId, false, "cancel", key, new { }, async (_, previous) =>
        {
            if (previous!.Engine != "MANUAL" || !IsOpen(previous.Status) && previous.Status != "CANCELADA") throw new ExecutionStateConflictException();
            if (previous.Status != "CANCELADA") await db.Database.GetDbConnection().ExecuteAsync(Command(
                "UPDATE erp.calculo_execucoes SET status='CANCELADA',concluida_em=@Now WHERE id=@Id",
                new { Now = clock.GetUtcNow().UtcDateTime, Id = previous.Id }, token));
            var row = (await GetAsync(previous.Id, token))!;
            return new Mutation<CalculationExecutionRow>(row, row.Id, null);
        }, receipt => GetAsync(receipt.ExecutionId, token), token);
    }

    public async Task<QuoteResultAggregate?> RecordManualQuoteAsync(Guid executionId, string key, RecordManualQuote input, CancellationToken token)
    {
        await new RecordManualQuoteCommandValidator().ValidateAndThrowAsync(new(executionId, key, input), token);
        var normalized = input with
        {
            Coverages = input.Coverages.OrderBy(x => x.ResultKey, StringComparer.Ordinal).ToArray(),
            Installments = input.Installments.OrderBy(x => x.OptionCode, StringComparer.Ordinal).ToArray()
        };
        return await MutateAsync(executionId, false, "manual-quote", key, normalized, async (calculation, execution) =>
        {
            if (execution!.Engine != "MANUAL" || !IsOpen(execution.Status)) throw new ExecutionStateConflictException();
            var ids = input.Coverages.Where(x => x.CoverageId.HasValue).Select(x => x.CoverageId!.Value).Distinct().ToArray();
            var count = await db.Database.GetDbConnection().ExecuteScalarAsync<int>(Command(
                "SELECT count(*)::int FROM erp.coberturas_catalogo WHERE id=ANY(@Ids) AND ramo_id=@Line AND ativo",
                new { Ids = ids, Line = calculation.InsuranceBranchId }, token));
            if (count != ids.Length) throw new ValidationException("Coberturas devolvidas incompatíveis com o ramo.");
            var quote = new QuoteResultRow
            {
                Id = Guid.NewGuid(), ExecutionId = executionId, InsurerQuoteNumber = input.InsurerQuoteNumber,
                TotalPremium = input.TotalPremium, NetPremium = input.NetPremium, Tax = input.Tax,
                InstallmentSurcharge = input.InstallmentSurcharge, CommissionAmount = input.CommissionAmount,
                ValidUntil = input.ValidUntil, ProposalLink = input.ProposalLink, InsurerMessage = input.InsurerMessage,
                Restrictions = input.Restrictions, InternalObservation = input.InternalObservation, ReceivedAtUtc = clock.GetUtcNow().UtcDateTime
            };
            await InsertAsync("cotacoes", quote, token);
            foreach (var c in input.Coverages) await InsertAsync("cotacao_coberturas", new QuoteCoverageRow
            {
                Id = Guid.NewGuid(), QuoteId = quote.Id, CoverageId = c.CoverageId, ResultKey = c.ResultKey,
                ExternalCode = c.ExternalCode, ReportedName = c.ReportedName, Included = c.Included, AcceptedLimit = c.AcceptedLimit,
                AcceptedFipePercentage = c.AcceptedFipePercentage, DeductibleType = c.DeductibleType, DeductibleAmount = c.DeductibleAmount,
                Premium = c.Premium, WaitingDays = c.WaitingDays, MandatoryParticipation = c.MandatoryParticipation,
                ClauseText = c.ClauseText, InsurerObservation = c.InsurerObservation, Order = c.Order
            }, token);
            foreach (var p in input.Installments) await InsertAsync("cotacao_parcelamentos", new QuoteInstallmentRow
            {
                Id = Guid.NewGuid(), QuoteId = quote.Id, OptionCode = p.OptionCode, PaymentMethod = p.PaymentMethod,
                InstallmentCount = p.InstallmentCount, DownPayment = p.DownPayment, InstallmentAmount = p.InstallmentAmount,
                TotalAmount = p.TotalAmount, InterestPercentage = p.InterestPercentage, InstallmentSurcharge = p.InstallmentSurcharge,
                FirstDueDate = p.FirstDueDate, Primary = p.Primary, Order = p.Order
            }, token);
            await db.Database.GetDbConnection().ExecuteAsync(Command("""
                UPDATE erp.calculo_execucoes SET status='CONCLUIDA',iniciada_em=COALESCE(iniciada_em,@Now),concluida_em=@Now WHERE id=@Id
                """, new { Now = quote.ReceivedAtUtc, Id = executionId }, token));
            return new Mutation<QuoteResultAggregate>((await GetQuoteAsync(quote.Id, token))!, executionId, quote.Id);
        }, receipt => GetQuoteAsync(receipt.QuoteId!.Value, token), token);
    }

    private async Task<T?> MutateAsync<T>(Guid resource, bool isCalculation, string operation, string key, object input,
        Func<CalculationRow, CalculationExecutionRow?, Task<Mutation<T>>> write, Func<Receipt, Task<T?>> replay, CancellationToken token) where T : class
    {
        await using var transaction = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(token) : null;
        await db.Database.OpenConnectionAsync(token);
        if (!user.IsAuthenticated || !Guid.TryParse(user.TenantId, out var tenant) || !Guid.TryParse(user.BranchId, out var branch))
            throw new UnauthorizedAccessException("Contexto de corretora obrigatório.");
        var connection = db.Database.GetDbConnection();
        var calculationId = isCalculation ? resource : await connection.QuerySingleOrDefaultAsync<Guid?>(Command(
            "SELECT calculo_id FROM erp.calculo_execucoes WHERE id=@Id", new { Id = resource }, token));
        if (!calculationId.HasValue) return null;
        var opportunityId = await connection.QuerySingleOrDefaultAsync<Guid?>(Command("""
            SELECT o.id FROM erp.oportunidades o JOIN erp.calculos c ON c.oportunidade_id=o.id
            WHERE c.id=@Id AND o.tenant_id=@Tenant AND o.filial_id=@Branch FOR UPDATE OF o
            """, new { Id = calculationId, Tenant = tenant, Branch = branch }, token));
        if (!opportunityId.HasValue || !await db.Opportunities.AnyAsync(x => x.Id == opportunityId.Value, token)) return null;
        var calculation = await connection.QuerySingleAsync<CalculationRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<CalculationRow>("c")} FROM erp.calculos c WHERE c.id=@Id", new { Id = calculationId }, token));
        var hash = Hash(input);
        var receipt = await connection.QuerySingleOrDefaultAsync<Receipt>(Command("""
            SELECT execution_id AS "ExecutionId",quote_id AS "QuoteId",request_hash AS "RequestHash" FROM infrastructure.execution_receipts
            WHERE tenant_id=@Tenant AND branch_id=@Branch AND operation=@Operation AND resource_id=@Resource AND idempotency_key=@Key
            """, new { Tenant = tenant, Branch = branch, Operation = operation, Resource = resource, Key = key }, token));
        if (receipt is not null)
        {
            if (receipt.RequestHash != hash) throw new CalculationConflictException();
            var existing = await replay(receipt);
            if (transaction is not null) await transaction.CommitAsync(token);
            return existing;
        }
        var previous = isCalculation ? null : await GetAsync(resource, token);
        if (!isCalculation && previous is null) return null;
        var result = await write(calculation, previous);
        await connection.ExecuteAsync(Command("""
            INSERT INTO infrastructure.execution_receipts (tenant_id,branch_id,operation,resource_id,idempotency_key,request_hash,execution_id,quote_id)
            VALUES (@Tenant,@Branch,@Operation,@Resource,@Key,@Hash,@Execution,CAST(@Quote AS uuid))
            """, new { Tenant = tenant, Branch = branch, Operation = operation, Resource = resource, Key = key, Hash = hash, Execution = result.ExecutionId, Quote = result.QuoteId }, token));
        await audit.WriteAsync(key, "Calculations", operation, result.QuoteId.HasValue ? "QuoteResultRow" : "CalculationExecutionRow",
            (result.QuoteId ?? result.ExecutionId).ToString(), "Canonical manual execution operation", token, tenant.ToString(), branch.ToString());
        if (transaction is not null) await transaction.CommitAsync(token);
        return result.Value;
    }

    private async Task<CalculationExecutionRow> InsertExecutionAsync(CalculationRow calculation, Guid insurer, decimal? commission, Guid? retryOf, CancellationToken token)
    {
        var connection = db.Database.GetDbConnection();
        if (!await connection.ExecuteScalarAsync<bool>(Command("""
            SELECT EXISTS(SELECT 1 FROM erp.seguradoras s JOIN erp.oportunidades o ON o.tenant_id=s.tenant_id
            WHERE o.id=@Opportunity AND s.id=@Insurer AND s.ativo)
            """, new { Opportunity = calculation.OpportunityId, Insurer = insurer }, token)))
            throw new ValidationException("Seguradora indisponível para esta oportunidade.");
        if (await connection.ExecuteScalarAsync<bool>(Command("""
            SELECT EXISTS(SELECT 1 FROM erp.calculo_execucoes WHERE calculo_id=@Id AND seguradora_id=@Insurer AND status IN ('AGUARDANDO','EM_EXECUCAO'))
            """, new { Id = calculation.Id, Insurer = insurer }, token))) throw new ExecutionStateConflictException();
        var attempt = await connection.ExecuteScalarAsync<int>(Command(
            "SELECT COALESCE(max(tentativa),0)+1 FROM erp.calculo_execucoes WHERE calculo_id=@Id AND seguradora_id=@Insurer",
            new { Id = calculation.Id, Insurer = insurer }, token));
        var row = new CalculationExecutionRow
        {
            Id = Guid.NewGuid(), CalculationId = calculation.Id, InsurerId = insurer, RetryOfId = retryOf, Attempt = attempt,
            AppliedCommission = commission ?? calculation.SuggestedCommission,
            CommissionOrigin = commission.HasValue ? "SOBRESCRITA" : "PADRAO_CALCULO", CreatedAtUtc = clock.GetUtcNow().UtcDateTime
        };
        await InsertAsync("calculo_execucoes", row, token);
        return row;
    }

    private async Task InsertAsync<T>(string table, T row, CancellationToken token)
    {
        var insert = CanonicalCalculationSql.Insert(table, row);
        await db.Database.GetDbConnection().ExecuteAsync(Command(insert.Sql, insert.Parameters, token));
    }
    public async Task<CalculationExecutionRow?> GetAsync(Guid executionId, CancellationToken token)
    {
        await db.Database.OpenConnectionAsync(token);
        var row = await db.Database.GetDbConnection().QuerySingleOrDefaultAsync<CalculationExecutionRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<CalculationExecutionRow>("e")} FROM erp.calculo_execucoes e WHERE e.id=@Id", new { Id = executionId }, token));
        return row is not null && await CanReadCalculationAsync(row.CalculationId, token) ? row : null;
    }
    private async Task<bool> CanReadCalculationAsync(Guid calculationId, CancellationToken token)
    {
        await db.Database.OpenConnectionAsync(token);
        var opportunity = await db.Database.GetDbConnection().QuerySingleOrDefaultAsync<Guid?>(Command(
            "SELECT oportunidade_id FROM erp.calculos WHERE id=@Id", new { Id = calculationId }, token));
        return opportunity.HasValue && await db.Opportunities.AnyAsync(x => x.Id == opportunity.Value, token);
    }
    public async Task<QuoteResultAggregate?> GetQuoteAsync(Guid quoteId, CancellationToken token)
    {
        await db.Database.OpenConnectionAsync(token);
        var connection = db.Database.GetDbConnection();
        var quote = await connection.QuerySingleOrDefaultAsync<QuoteResultRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<QuoteResultRow>("q")} FROM erp.cotacoes q WHERE q.id=@Id", new { Id = quoteId }, token));
        if (quote is null) return null;
        var execution = await GetAsync(quote.ExecutionId, token);
        if (execution is null) return null;
        var coverages = await connection.QueryAsync<QuoteCoverageRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<QuoteCoverageRow>("c")} FROM erp.cotacao_coberturas c WHERE c.cotacao_id=@Id ORDER BY c.ordem NULLS LAST,c.chave_resultado", new { Id = quoteId }, token));
        var installments = await connection.QueryAsync<QuoteInstallmentRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<QuoteInstallmentRow>("p")} FROM erp.cotacao_parcelamentos p WHERE p.cotacao_id=@Id ORDER BY p.ordem NULLS LAST,p.codigo_opcao", new { Id = quoteId }, token));
        return new(quote, execution, coverages.AsList(), installments.AsList());
    }
    public async Task<PagedResult<CalculationExecutionRow>?> ListAsync(Guid calculationId, int page, int pageSize, CancellationToken token)
    {
        if (!await CanReadCalculationAsync(calculationId, token)) return null;
        var parameters = new { Id = calculationId, Limit = pageSize, Offset = (long)(page - 1) * pageSize };
        var count = await db.Database.GetDbConnection().ExecuteScalarAsync<long>(Command("SELECT count(*) FROM erp.calculo_execucoes WHERE calculo_id=@Id", parameters, token));
        var rows = await db.Database.GetDbConnection().QueryAsync<CalculationExecutionRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<CalculationExecutionRow>("e")} FROM erp.calculo_execucoes e WHERE e.calculo_id=@Id ORDER BY e.criada_em DESC,e.id LIMIT @Limit OFFSET @Offset", parameters, token));
        return new(rows.AsList(), page, pageSize, count);
    }
    public async Task<PagedResult<QuoteResultAggregate>?> ListQuotesAsync(Guid opportunityId, int page, int pageSize, CancellationToken token)
    {
        if (!await db.Opportunities.AnyAsync(x => x.Id == opportunityId, token)) return null;
        await db.Database.OpenConnectionAsync(token);
        var parameters = new { Id = opportunityId, Limit = pageSize, Offset = (long)(page - 1) * pageSize };
        const string source = "FROM erp.cotacoes q JOIN erp.calculo_execucoes e ON e.id=q.execucao_id JOIN erp.calculos c ON c.id=e.calculo_id WHERE c.oportunidade_id=@Id";
        var connection = db.Database.GetDbConnection();
        var count = await connection.ExecuteScalarAsync<long>(Command("SELECT count(*) " + source, parameters, token));
        var ids = await connection.QueryAsync<Guid>(Command("SELECT q.id " + source + " ORDER BY q.recebida_em DESC NULLS LAST,q.id LIMIT @Limit OFFSET @Offset", parameters, token));
        var rows = new List<QuoteResultAggregate>();
        foreach (var id in ids) { var row = await GetQuoteAsync(id, token); if (row is not null) rows.Add(row); }
        return new(rows, page, pageSize, count);
    }
    private static bool IsOpen(string status) => status is "AGUARDANDO" or "EM_EXECUCAO";
    // JSON is transient hashing only. Normalize decimal scale recursively; no payload is persisted.
    private static string Hash(object value)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(value));
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer)) WriteCanonical(writer, document.RootElement);
        return Convert.ToHexString(SHA256.HashData(buffer.ToArray()));
    }
    private static void WriteCanonical(Utf8JsonWriter writer, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var p in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal)) { writer.WritePropertyName(p.Name); WriteCanonical(writer, p.Value); }
                writer.WriteEndObject(); break;
            case JsonValueKind.Array:
                writer.WriteStartArray(); foreach (var e in element.EnumerateArray()) WriteCanonical(writer, e); writer.WriteEndArray(); break;
            case JsonValueKind.Number:
                writer.WriteRawValue(element.GetDecimal().ToString("G29", System.Globalization.CultureInfo.InvariantCulture)); break;
            default: element.WriteTo(writer); break;
        }
    }
    private sealed record Mutation<T>(T Value, Guid ExecutionId, Guid? QuoteId);
    private sealed record Receipt(Guid ExecutionId, Guid? QuoteId, string RequestHash);
}
