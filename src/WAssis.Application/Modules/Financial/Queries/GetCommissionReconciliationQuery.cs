using MediatR;
using WAssis.Application.Modules.Financial.Dtos;

namespace WAssis.Application.Modules.Financial.Queries;

public sealed record GetCommissionReconciliationQuery(Guid ReconciliationId) : IRequest<CommissionReconciliationDto?>;
