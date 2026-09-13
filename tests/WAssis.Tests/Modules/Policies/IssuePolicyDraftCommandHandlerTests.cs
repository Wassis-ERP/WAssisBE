using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Policies.Commands;
using WAssis.Application.Modules.Policies.Interfaces;
using WAssis.Domain.Modules.Policies.Entities;
using WAssis.Domain.Modules.Policies.Enums;

namespace WAssis.Tests.Modules.Policies;

public sealed class IssuePolicyDraftCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMarkDraftAsIssued_AndGeneratePolicyNumber_WhenNumberIsMissing()
    {
        var draft = PolicyDraft.Create(
            "tenant-issue",
            Guid.NewGuid(),
            "corr-pol-issue-1",
            "Allianz",
            "PROP-9090",
            "Cliente Teste",
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddYears(1),
            1500m,
            150m,
            "pronto");

        var repository = new InMemoryPolicyDraftRepository(draft);
        var auditTrailWriter = new FakeAuditTrailWriter();
        var handler = new IssuePolicyDraftCommandHandler(repository, auditTrailWriter);

        var result = await handler.Handle(new IssuePolicyDraftCommand(draft.Id, null, "emitir"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(PolicyDraftStatus.Issued, result.Value!.Status);
        Assert.StartsWith("WASSIS-", result.Value.PolicyNumber);
        Assert.NotNull(result.Value.IssuedAtUtc);
        Assert.Single(auditTrailWriter.Entries);
    }

    private sealed class InMemoryPolicyDraftRepository(PolicyDraft draft) : IPolicyDraftRepository
    {
        public Task AddAsync(PolicyDraft item, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task<PolicyDraft?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(id == draft.Id ? draft : null);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeAuditTrailWriter : IAuditTrailWriter
    {
        public List<string> Entries { get; } = [];

        public Task WriteAsync(
            string correlationId,
            string module,
            string action,
            string entityType,
            string entityId,
            string? notes,
            CancellationToken cancellationToken, string? tenantId = null)
        {
            Entries.Add($"{module}:{action}:{entityType}:{entityId}:{notes}");
            return Task.CompletedTask;
        }
    }
}
