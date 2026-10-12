using FluentValidation;
using WAssis.Application.Modules.Documents.Commands;

namespace WAssis.Application.Modules.Documents.Validators;

public sealed class UploadProposalDocumentCommandValidator : AbstractValidator<UploadProposalDocumentCommand>
{
    public UploadProposalDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
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
        RuleFor(x => x.StorageKey).NotEmpty().MaximumLength(160);
    }
}
