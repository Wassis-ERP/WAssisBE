using Microsoft.AspNetCore.Mvc;
using WAssis.Services.Api.Modules.Migration.Contracts;

namespace WAssis.Services.Api.Modules.Migration;

public static class MigrationProblemDetailsFactory
{
    public static ObjectResult Pending(ControllerBase controller, FrontendContractGapViewModel gap, string detail)
    {
        var problem = new ProblemDetails
        {
            Title = "Contrato em migracao para o WAssisBE",
            Detail = detail,
            Status = StatusCodes.Status501NotImplemented,
            Type = "https://docs.wassis.local/problems/frontend-contract-pending"
        };

        problem.Extensions["code"] = "frontend_contract.pending";
        problem.Extensions["contractKey"] = gap.ContractKey;
        problem.Extensions["legacySurface"] = gap.LegacySurface;
        problem.Extensions["ownerIssue"] = gap.OwnerIssue;
        problem.Extensions["nextStep"] = gap.NextStep;

        if (!string.IsNullOrWhiteSpace(gap.ReplacementEndpoint))
        {
            problem.Extensions["replacementEndpoint"] = gap.ReplacementEndpoint;
        }

        return controller.StatusCode(StatusCodes.Status501NotImplemented, problem);
    }
}
