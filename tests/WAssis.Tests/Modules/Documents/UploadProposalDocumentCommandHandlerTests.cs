using WAssis.Application.Modules.Documents.Commands;
using WAssis.Application.Modules.Documents.Interfaces;
using WAssis.Domain.Modules.Documents.Entities;
using WAssis.Domain.Modules.Documents.Enums;
using WAssis.Tests.TestDoubles;

namespace WAssis.Tests.Modules.Documents;

public sealed class UploadProposalDocumentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldMarkDocumentAsFailed_WhenNoTextAndNoOcr()
    {
        var repository = new InMemoryImportedDocumentRepository();
        var handler = new UploadProposalDocumentCommandHandler(
            new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                TenantId = "tenant-docs"
            },
            repository,
            new EmptyPdfTextExtractor(),
            new EmptyOcrTextExtractor(),
            new FakeProposalDocumentParser());

        var result = await handler.Handle(
            new UploadProposalDocumentCommand(
                "corr-doc-1",
                "proposta.pdf",
                "application/pdf",
                "manual_upload",
                [1, 2, 3]),
            CancellationToken.None);

        Assert.Equal(ImportedDocumentStatus.Failed, result.Status);
        Assert.Contains("OCR", result.ParsingNotes);
        Assert.Equal("tenant-docs", repository.Items.Single().TenantId);
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

    private sealed class EmptyPdfTextExtractor : IPdfTextExtractor
    {
        public Task<string> ExtractTextAsync(byte[] content, CancellationToken cancellationToken)
        {
            return Task.FromResult(string.Empty);
        }
    }

    private sealed class EmptyOcrTextExtractor : IOcrTextExtractor
    {
        public Task<string?> ExtractTextAsync(byte[] content, CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(null);
        }
    }

    private sealed class FakeProposalDocumentParser : IProposalDocumentParser
    {
        public Application.Modules.Documents.Dtos.ProposalDocumentParsingResultDto Parse(string extractedText)
        {
            return new Application.Modules.Documents.Dtos.ProposalDocumentParsingResultDto(
                "proposal_pdf",
                extractedText,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                0.35m,
                true,
                null);
        }
    }
}
