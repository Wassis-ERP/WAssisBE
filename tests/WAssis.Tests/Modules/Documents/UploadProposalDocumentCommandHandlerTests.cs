using WAssis.Application.Modules.Documents.Commands;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Domain.Modules.Documents.Enums;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Documents;

public sealed class UploadProposalDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldQueueStoredDocumentWithoutParsingInRequest()
    {
        var repository = new InMemoryImportedDocumentRepository();
        var handler = new UploadProposalDocumentCommandHandler(
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = "22222222-2222-2222-2222-222222222222",
                BranchId = "44444444-4444-4444-4444-444444444444",
                BranchIds = ["44444444-4444-4444-4444-444444444444"]
            },
            repository);

        var id = Guid.NewGuid();
        var result = await handler.Handle(
            new UploadProposalDocumentCommand(
                id,
                "corr-doc-1",
                "proposta.pdf",
                "application/pdf",
                "manual_upload",
                "tenant/branch/document.pdf"),
            CancellationToken.None);

        Assert.Equal(id, result.Id);
        Assert.Equal(ImportedDocumentStatus.Received, result.Status);
        Assert.Equal("22222222-2222-2222-2222-222222222222", repository.Items.Single().TenantId);
    }

    private sealed class InMemoryImportedDocumentRepository : IImportedDocumentRepository
    {
        private readonly List<ImportedDocument> _items = [];

        public IReadOnlyCollection<ImportedDocument> Items => _items;

        public Task AddAsync(ImportedDocument document, CancellationToken cancellationToken)
        {
            _items.Add(document);
            return Task.CompletedTask;
        }

        public Task<ImportedDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_items.SingleOrDefault(x => x.Id == id));
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}
