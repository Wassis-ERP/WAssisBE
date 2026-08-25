using WAssis.Application.Modules.Documents.Commands;
using WAssis.Application.Modules.Documents.Validators;

namespace WAssis.Tests.Modules.Documents;

public sealed class UploadProposalDocumentCommandValidatorTests
{
    private readonly UploadProposalDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldAcceptPdfSignature()
    {
        var command = new UploadProposalDocumentCommand(
            "corr-pdf",
            "proposta.pdf",
            "application/pdf",
            "manual_upload",
            "%PDF-1.7 test"u8.ToArray());

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldRejectSpoofedPdf()
    {
        var command = new UploadProposalDocumentCommand(
            "corr-spoof",
            "proposta.pdf",
            "application/pdf",
            "manual_upload",
            "not-a-pdf"u8.ToArray());

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("assinatura PDF", StringComparison.Ordinal));
    }
}
