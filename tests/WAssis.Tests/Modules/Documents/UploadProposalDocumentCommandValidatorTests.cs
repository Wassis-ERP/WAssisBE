using WAssis.Application.Modules.Documents.Commands;
using WAssis.Application.Modules.Documents.Validators;

namespace WAssis.Tests.Modules.Documents;

public sealed class UploadProposalDocumentCommandValidatorTests
{
    private readonly UploadProposalDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldAcceptStoredPdfMetadata()
    {
        var command = new UploadProposalDocumentCommand(
            Guid.NewGuid(),
            "corr-pdf",
            "proposta.pdf",
            "application/pdf",
            "manual_upload",
            "tenant/branch/document.pdf");

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldRejectInvalidMetadata()
    {
        var command = new UploadProposalDocumentCommand(
            Guid.Empty,
            "corr-spoof",
            "proposta.exe",
            "application/octet-stream",
            "manual_upload",
            "");

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.DocumentId));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(command.StorageKey));
    }
}
