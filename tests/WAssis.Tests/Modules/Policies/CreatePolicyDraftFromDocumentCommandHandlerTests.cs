using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Application.Modules.Policies.Commands;
using WAssis.Application.Modules.Policies.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Domain.Modules.Documents.Enums;
using WAssis.Domain.Modules.Policies.Entities;
using WAssis.Domain.Modules.Policies.Enums;

namespace WAssis.Tests.Modules.Policies;

public sealed class CreatePolicyDraftFromDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateDraftFromParsedDocument()
    {
        var document = ImportedDocument.Create("corr-pol-1", "proposta.pdf", "application/pdf", "manual_upload");
        document.MarkAsParsed(
            "proposal_pdf",
            "texto",
            "Allianz",
            "PROP-1",
            "Cliente Teste",
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddYears(1),
            1000m,
            100m,
            "ok");

        var handler = new CreatePolicyDraftFromDocumentCommandHandler(
            new InMemoryImportedDocumentRepository(document),
            new InMemoryPolicyDraftRepository());

        var result = await handler.Handle(new CreatePolicyDraftFromDocumentCommand(document.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(document.Id, result.Value!.ImportedDocumentId);
        Assert.Equal("PROP-1", result.Value.ProposalNumber);
        Assert.Equal(PolicyDraftStatus.Draft, result.Value.Status);
    }

    private sealed class InMemoryImportedDocumentRepository(ImportedDocument document) : IImportedDocumentRepository
    {
        public Task AddAsync(ImportedDocument importedDocument, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<ImportedDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(id == document.Id ? document : null);
        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class InMemoryPolicyDraftRepository : IPolicyDraftRepository
    {
        private readonly List<PolicyDraft> _items = [];

        public Task AddAsync(PolicyDraft draft, CancellationToken cancellationToken)
        {
            _items.Add(draft);
            return Task.CompletedTask;
        }

        public Task<PolicyDraft?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_items.SingleOrDefault(x => x.Id == id));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
