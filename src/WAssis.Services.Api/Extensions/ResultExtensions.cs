using Microsoft.AspNetCore.Mvc;
using WAssis.Domain.Core.Messages;

namespace WAssis.Services.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<TValue>(
        this ControllerBase controller,
        Result<TValue> result,
        Func<TValue, IActionResult> onSuccess)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return onSuccess(result.Value);
        }

        var payload = new
        {
            error = result.Error?.Code,
            message = result.Error?.Description
        };

        return result.Error?.Type switch
        {
            ErrorType.NotFound => controller.NotFound(payload),
            ErrorType.Conflict => controller.Conflict(payload),
            ErrorType.Validation => controller.BadRequest(payload),
            _ => controller.BadRequest(payload)
        };
    }
}
