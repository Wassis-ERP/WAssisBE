using WAssis.Application.Modules.Notifications.Dtos;

namespace WAssis.Application.Modules.Notifications.Interfaces;

public interface IOperationsDashboardReadRepository
{
    Task<OperationsDashboardDto> GetOverviewAsync(CancellationToken cancellationToken);
}
