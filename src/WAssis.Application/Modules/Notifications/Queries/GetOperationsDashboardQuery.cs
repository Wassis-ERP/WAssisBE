using MediatR;
using WAssis.Application.Modules.Notifications.Dtos;

namespace WAssis.Application.Modules.Notifications.Queries;

public sealed record GetOperationsDashboardQuery() : IRequest<OperationsDashboardDto>;
