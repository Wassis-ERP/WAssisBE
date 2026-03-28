using MediatR;
using WAssis.Application.Modules.Billing.Dtos;
using WAssis.Application.Modules.Billing.Interfaces;

namespace WAssis.Application.Modules.Billing.Queries;

public sealed class GetBillingInvoiceQueryHandler(IBillingRepository repository)
    : IRequestHandler<GetBillingInvoiceQuery, BillingInvoiceDto?>
{
    public async Task<BillingInvoiceDto?> Handle(GetBillingInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoice = await repository.GetInvoiceByIdAsync(request.Id, cancellationToken);
        return invoice?.ToDto();
    }
}
