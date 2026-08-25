using FluentValidation;
using WAssis.Application.Modules.Documents.Commands;

namespace WAssis.Application.Modules.Documents.Validators;

public sealed class UploadProposalDocumentCommandValidator : AbstractValidator<UploadProposalDocumentCommand>
{
    private const int MaxPdfBytes = 20_000_000;
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();

    public UploadProposalDocumentCommandValidator()
    {
        RuleFor(x => x.CorrelationId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(static fileName => string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Somente arquivos PDF sao aceitos.");
        RuleFor(x => x.ContentType)
            .Equal("application/pdf", StringComparer.OrdinalIgnoreCase)
            .WithMessage("O Content-Type deve ser application/pdf.");
        RuleFor(x => x.Source).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Content)
            .NotEmpty()
            .Must(static content => content.Length <= MaxPdfBytes)
            .WithMessage($"O PDF deve ter no maximo {MaxPdfBytes} bytes.")
            .Must(static content => content.AsSpan().StartsWith(PdfSignature))
            .WithMessage("O arquivo enviado nao possui uma assinatura PDF valida.");
    }
}
