using FluentValidation;
using WAssis.Application.Modules.Documents.Commands;

namespace WAssis.Application.Modules.Documents.Validators;

public sealed class UploadProposalDocumentCommandValidator : AbstractValidator<UploadProposalDocumentCommand>
{
    public UploadProposalDocumentCommandValidator()
    {
        RuleFor(x => x.CorrelationId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Source).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Content).NotEmpty();
    }
}
