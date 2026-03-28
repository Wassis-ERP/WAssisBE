using MediatR;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Application.Modules.Billing.Interfaces;
using WAssis.Domain.Core.Messages;
using WAssis.Domain.Modules.Billing.Entities;
using WAssis.Domain.Modules.Billing.Enums;

namespace WAssis.Application.Modules.Billing.Commands;

public sealed class IssueBillingInvoiceCommandHandler(IBillingRepository repository)
    : IRequestHandler<IssueBillingInvoiceCommand, Result<BillingInvoiceDto>>
{
    public async Task<Result<BillingInvoiceDto>> Handle(IssueBillingInvoiceCommand request, CancellationToken cancellationToken)
    {
        var subscription = await repository.GetSubscriptionByIdAsync(request.BillingSubscriptionId, cancellationToken);
        if (subscription is null)
        {
            return Result<BillingInvoiceDto>.Failure(new Error("billing.subscription.not_found", "Assinatura de billing não encontrada.", ErrorType.NotFound));
        }

        if (subscription.Status != BillingSubscriptionStatus.Active)
        {
            return Result<BillingInvoiceDto>.Failure(new Error("billing.subscription.inactive", "A assinatura não está ativa para faturamento.", ErrorType.Conflict));
        }

        var normalizedReferencePeriod = request.ReferencePeriod.Trim();
        if (await repository.InvoiceReferencePeriodExistsAsync(subscription.Id, normalizedReferencePeriod, cancellationToken))
        {
            return Result<BillingInvoiceDto>.Failure(new Error("billing.invoice.duplicate_period", "Já existe fatura emitida para este período de referência.", ErrorType.Conflict));
        }

        var dueDateUtc = request.DueDateUtc ?? subscription.NextInvoiceDueDateUtc;
        var invoice = BillingInvoice.Create(
            subscription.TenantId,
            subscription.Id,
            request.CorrelationId,
            normalizedReferencePeriod,
            subscription.Amount,
            dueDateUtc,
            request.ExternalReference,
            request.Notes);

        subscription.RegisterInvoiceIssued(dueDateUtc);

        await repository.AddInvoiceAsync(invoice, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Result<BillingInvoiceDto>.Success(invoice.ToDto());
    }
}
