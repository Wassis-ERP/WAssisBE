using MediatR;
using WAssis.Application.Modules.Financial.Dtos;

namespace WAssis.Application.Modules.Financial.Commands;

public sealed record SettleCommissionReconciliationCommand(
    Guid ReconciliationId,
    string? MatchedReference,
    string? SettlementNotes) : IRequest<CommissionReconciliationDto?>;
