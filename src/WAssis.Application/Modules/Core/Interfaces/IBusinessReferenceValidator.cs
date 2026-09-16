namespace WAssis.Application.Modules.Core.Interfaces;

public interface IBusinessReferenceValidator
{
    Task ValidateProducerReferencesAsync(
        string branchId,
        string? producerId,
        string? managerId,
        CancellationToken cancellationToken);

    Task ValidateResponsibleAsync(
        string branchId,
        string responsibleId,
        CancellationToken cancellationToken);
}
