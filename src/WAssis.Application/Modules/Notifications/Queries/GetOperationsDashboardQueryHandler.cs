using MediatR;
using WAssis.Application.Modules.Notifications.Dtos;
using WAssis.Application.Modules.Notifications.Interfaces;

namespace WAssis.Application.Modules.Notifications.Queries;

public sealed class GetOperationsDashboardQueryHandler(IOperationsDashboardReadRepository repository)
    : IRequestHandler<GetOperationsDashboardQuery, OperationsDashboardDto>
{
    public Task<OperationsDashboardDto> Handle(GetOperationsDashboardQuery request, CancellationToken cancellationToken)
    {
        return repository.GetOverviewAsync(cancellationToken);
    }
}
