using MediatR;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Application.Modules.Billing.Interfaces;
using WAssis.Domain.Core.Messages;
using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Application.Modules.Billing.Commands;

public sealed class MarkBillingInvoicePaidCommandHandler(IBillingRepository repository)
    : IRequestHandler<MarkBillingInvoicePaidCommand, Result<BillingInvoiceDto>>
{
    public async Task<Result<BillingInvoiceDto>> Handle(MarkBillingInvoicePaidCommand request, CancellationToken cancellationToken)
    {
        var invoice = await repository.GetInvoiceByIdAsync(request.BillingInvoiceId, cancellationToken);
        if (invoice is null)
        {
            return Result<BillingInvoiceDto>.Failure(new Error("billing.invoice.not_found", "Fatura de billing não encontrada.", ErrorType.NotFound));
        }

        if (invoice.Status == BillingInvoiceStatus.Paid)
        {
            return Result<BillingInvoiceDto>.Failure(new Error("billing.invoice.already_paid", "A fatura já foi marcada como paga.", ErrorType.Conflict));
        }

        invoice.MarkPaid(request.PaymentMethod, request.ExternalReference, request.Notes);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<BillingInvoiceDto>.Success(invoice.ToDto());
    }
}
