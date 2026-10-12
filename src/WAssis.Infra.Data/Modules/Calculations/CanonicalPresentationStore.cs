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

public sealed class CanonicalPresentationStore(WAssisDbContext db, ICurrentUserContext user, TimeProvider clock,
    IAuditTrailWriter audit, ICanonicalExecutionStore executions) : ICanonicalPresentationStore
{
    private CommandDefinition Command(string sql, object? parameters, CancellationToken token) =>
        new(sql, parameters, db.Database.CurrentTransaction?.GetDbTransaction(), commandTimeout: 30, cancellationToken: token);

    public async Task<CommercialPresentationAggregate?> SaveAsync(Guid resourceId, bool update, string key, SaveCommercialPresentation input, CancellationToken token)
    {
        await new SavePresentationCommandValidator().ValidateAndThrowAsync(new(resourceId, update, key, input), token);
        await using var transaction = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(token) : null;
        await db.Database.OpenConnectionAsync(token);
        if (!user.IsAuthenticated || !Guid.TryParse(user.TenantId, out var tenant) || !Guid.TryParse(user.BranchId, out var branch))
            throw new UnauthorizedAccessException("Contexto de corretora obrigatório.");
        var connection = db.Database.GetDbConnection();
        var opportunityId = update ? await connection.QuerySingleOrDefaultAsync<Guid?>(Command(
            "SELECT oportunidade_id FROM erp.apresentacoes_comerciais WHERE id=@Id", new { Id = resourceId }, token)) : resourceId;
        if (!opportunityId.HasValue) return null;
        var locked = await connection.QuerySingleOrDefaultAsync<Guid?>(Command("""
            SELECT id FROM erp.oportunidades WHERE id=@Id AND tenant_id=@Tenant AND filial_id=@Branch FOR UPDATE
            """, new { Id = opportunityId, Tenant = tenant, Branch = branch }, token));
        if (!locked.HasValue || !await db.Opportunities.AnyAsync(x => x.Id == opportunityId.Value, token)) return null;
        var operation = update ? "update" : "create";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(input))));
        var receipt = await connection.QuerySingleOrDefaultAsync<Receipt>(Command("""
            SELECT presentation_id AS "PresentationId",revision AS "Revision",request_hash AS "RequestHash" FROM infrastructure.presentation_receipts
            WHERE tenant_id=@Tenant AND branch_id=@Branch AND operation=@Operation AND resource_id=@Resource AND idempotency_key=@Key
            """, new { Tenant = tenant, Branch = branch, Operation = operation, Resource = resourceId, Key = key }, token));
        if (receipt is not null)
        {
            var existing = await GetAsync(receipt.PresentationId, token);
            if (receipt.RequestHash != hash || existing?.Revision != receipt.Revision) throw new PresentationConflictException();
            if (transaction is not null) await transaction.CommitAsync(token);
            return existing;
        }
        var previous = update ? await GetAsync(resourceId, token) : null;
        if (update && previous is null) return null;
        if (update && previous!.Revision != input.ExpectedRevision) throw new PresentationConflictException();
        var candidates = new Dictionary<Guid, PresentationResult>();
        foreach (var item in input.Items)
        {
            var quote = await executions.GetQuoteAsync(item.QuoteId, token);
            if (quote is null) throw new ValidationException("Cotação indisponível nesta oportunidade.");
            var calculation = await connection.QuerySingleAsync<CalculationRow>(Command(
                $"SELECT {CanonicalCalculationSql.Select<CalculationRow>("c")} FROM erp.calculos c WHERE c.id=@Id", new { Id = quote.Execution.CalculationId }, token));
            if (calculation.OpportunityId != opportunityId) throw new ValidationException("Cotação indisponível nesta oportunidade.");
            if (item.InstallmentId.HasValue && !quote.Installments.Any(p => p.Id == item.InstallmentId))
                throw new ValidationException("Parcelamento deve pertencer à cotação.");
            candidates.Add(item.QuoteId, new(quote, calculation, string.Empty));
        }
        Guid? creator = previous?.Presentation.CreatedById;
        if (!update)
        {
            if (!Guid.TryParse(user.UserId, out var userId) || !await connection.ExecuteScalarAsync<bool>(Command(
                "SELECT EXISTS(SELECT 1 FROM erp.profiles WHERE id=@Id AND tenant_id=@Tenant AND ativo)", new { Id = userId, Tenant = tenant }, token)))
                throw new UnauthorizedAccessException("Autor da apresentação indisponível.");
            creator = userId;
        }
        var now = clock.GetUtcNow().UtcDateTime;
        var header = new CommercialPresentationRow
        {
            Id = previous?.Presentation.Id ?? Guid.NewGuid(), OpportunityId = opportunityId.Value, CreatedById = creator,
            Title = input.Title, Layout = input.Layout, Ordering = input.Ordering, ShowFipe = input.ShowFipe,
            ShowAdvantages = input.ShowAdvantages, ShowLegend = input.ShowLegend, ShowNotes = input.ShowNotes,
            ShowCommission = input.ShowCommission, CommercialNotes = input.CommercialNotes, Status = input.Status,
            SelectedQuoteId = input.SelectedQuoteId, CreatedAtUtc = previous?.Presentation.CreatedAtUtc ?? now, UpdatedAtUtc = now,
            GeneratedAtUtc = input.Status == "GERADA" ? previous?.Presentation.GeneratedAtUtc ?? now : null,
            SelectedAtUtc = input.SelectedQuoteId.HasValue ? previous?.Presentation.SelectedQuoteId == input.SelectedQuoteId ? previous.Presentation.SelectedAtUtc ?? now : now : null
        };
        if (update)
        {
            var command = CanonicalCalculationSql.Update("apresentacoes_comerciais", header, new HashSet<string> { "id", "oportunidade_id", "criado_por_id", "criado_em" });
            await connection.ExecuteAsync(Command(command.Sql, command.Parameters, token));
            await connection.ExecuteAsync(Command("DELETE FROM erp.apresentacao_cotacoes WHERE apresentacao_id=@Id", new { Id = header.Id }, token));
            await connection.ExecuteAsync(Command("UPDATE infrastructure.presentation_states SET revision=revision+1 WHERE presentation_id=@Id", new { Id = header.Id }, token));
        }
        else
        {
            await InsertAsync("apresentacoes_comerciais", header, token);
            await connection.ExecuteAsync(Command("INSERT INTO infrastructure.presentation_states (presentation_id,revision) VALUES (@Id,1)", new { Id = header.Id }, token));
        }
        var ordered = OrderItems(input, candidates);
        for (var i = 0; i < ordered.Length; i++)
        {
            var item = ordered[i];
            await InsertAsync("apresentacao_cotacoes", new PresentationQuoteRow
            {
                Id = Guid.NewGuid(), PresentationId = header.Id, QuoteId = item.QuoteId, InstallmentId = item.InstallmentId,
                Recommended = item.Recommended, CommercialTitle = item.CommercialTitle, Advantages = item.Advantages,
                CommercialNote = item.CommercialNote, Order = (i + 1) * 10
            }, token);
        }
        var revision = (previous?.Revision ?? 0) + 1;
        await connection.ExecuteAsync(Command("""
            INSERT INTO infrastructure.presentation_receipts (tenant_id,branch_id,operation,resource_id,idempotency_key,request_hash,presentation_id,revision)
            VALUES (@Tenant,@Branch,@Operation,@Resource,@Key,@Hash,@Presentation,@Revision)
            """, new { Tenant = tenant, Branch = branch, Operation = operation, Resource = resourceId, Key = key, Hash = hash, Presentation = header.Id, Revision = revision }, token));
        await audit.WriteAsync(key, "Calculations", update ? "UpdatedPresentation" : "CreatedPresentation", "CommercialPresentationRow",
            header.Id.ToString(), "Commercial comparison saved", token, tenant.ToString(), branch.ToString());
        var result = await GetAsync(header.Id, token);
        if (transaction is not null) await transaction.CommitAsync(token);
        return result;
    }

    private static PresentationQuoteInput[] OrderItems(SaveCommercialPresentation input, IReadOnlyDictionary<Guid, PresentationResult> candidates)
    {
        var items = input.Items;
        return input.Ordering switch
        {
            "PREMIO" => items.OrderBy(x => candidates[x.QuoteId].Result.Quote.TotalPremium ?? decimal.MaxValue).ToArray(),
            "FRANQUIA" => items.OrderBy(x => candidates[x.QuoteId].Result.Coverages.FirstOrDefault(c => c.DeductibleAmount.HasValue)?.DeductibleAmount ?? decimal.MaxValue).ToArray(),
            "RECOMENDACAO" => items.OrderByDescending(x => x.Recommended).ThenBy(x => candidates[x.QuoteId].Result.Quote.TotalPremium ?? decimal.MaxValue).ToArray(),
            _ => items.ToArray()
        };
    }
    private async Task InsertAsync<T>(string table, T row, CancellationToken token)
    {
        var command = CanonicalCalculationSql.Insert(table, row);
        await db.Database.GetDbConnection().ExecuteAsync(Command(command.Sql, command.Parameters, token));
    }
    public async Task<CommercialPresentationAggregate?> GetAsync(Guid id, CancellationToken token)
    {
        await using var transaction = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync(token) : null;
        await db.Database.OpenConnectionAsync(token);
        var connection = db.Database.GetDbConnection();
        // Readers hold the same opportunity lock used by writers, so header/items/revision form one consistent view.
        var opportunity = await connection.QuerySingleOrDefaultAsync<Guid?>(Command("""
            SELECT o.id FROM erp.oportunidades o JOIN erp.apresentacoes_comerciais p ON p.oportunidade_id=o.id
            WHERE p.id=@Id FOR SHARE OF o
            """, new { Id = id }, token));
        if (!opportunity.HasValue || !await db.Opportunities.AnyAsync(x => x.Id == opportunity.Value, token)) return null;
        var header = await connection.QuerySingleOrDefaultAsync<CommercialPresentationRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<CommercialPresentationRow>("p")} FROM erp.apresentacoes_comerciais p WHERE p.id=@Id", new { Id = id }, token));
        if (header is null || !await db.Opportunities.AnyAsync(x => x.Id == header.OpportunityId, token)) return null;
        var items = await connection.QueryAsync<PresentationQuoteRow>(Command(
            $"SELECT {CanonicalCalculationSql.Select<PresentationQuoteRow>("i")} FROM erp.apresentacao_cotacoes i WHERE i.apresentacao_id=@Id ORDER BY i.ordem,i.id", new { Id = id }, token));
        var revision = await connection.ExecuteScalarAsync<int>(Command("SELECT revision FROM infrastructure.presentation_states WHERE presentation_id=@Id", new { Id = id }, token));
        var results = new List<PresentationResult>();
        foreach (var item in items)
        {
            var result = await executions.GetQuoteAsync(item.QuoteId, token);
            if (result is null) return null;
            var calculation = await connection.QuerySingleAsync<CalculationRow>(Command(
                $"SELECT {CanonicalCalculationSql.Select<CalculationRow>("c")} FROM erp.calculos c WHERE c.id=@Id", new { Id = result.Execution.CalculationId }, token));
            var insurerName = await connection.QuerySingleAsync<string>(Command("SELECT COALESCE(nome,'Seguradora sem nome') FROM erp.seguradoras WHERE id=@Id", new { Id = result.Execution.InsurerId }, token));
            results.Add(new(result, calculation, insurerName));
        }
        if (transaction is not null) await transaction.CommitAsync(token);
        return new(header, items.AsList(), revision, results);
    }
    public async Task<PagedResult<CommercialPresentationAggregate>?> ListAsync(Guid opportunityId, int page, int pageSize, CancellationToken token)
    {
        if (!await db.Opportunities.AnyAsync(x => x.Id == opportunityId, token)) return null;
        await db.Database.OpenConnectionAsync(token);
        var parameters = new { Id = opportunityId, Limit = pageSize, Offset = (long)(page - 1) * pageSize };
        var connection = db.Database.GetDbConnection();
        var count = await connection.ExecuteScalarAsync<long>(Command("SELECT count(*) FROM erp.apresentacoes_comerciais WHERE oportunidade_id=@Id", parameters, token));
        var ids = await connection.QueryAsync<Guid>(Command("SELECT id FROM erp.apresentacoes_comerciais WHERE oportunidade_id=@Id ORDER BY atualizado_em DESC NULLS LAST,id LIMIT @Limit OFFSET @Offset", parameters, token));
        var rows = new List<CommercialPresentationAggregate>();
        foreach (var id in ids) { var row = await GetAsync(id, token); if (row is not null) rows.Add(row); }
        return new(rows, page, pageSize, count);
    }
    private sealed record Receipt(Guid PresentationId, int Revision, string RequestHash);
}
