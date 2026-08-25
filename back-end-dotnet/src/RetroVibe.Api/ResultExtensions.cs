using Microsoft.AspNetCore.Mvc;
using RetroVibe.Domain.Kernel;

namespace RetroVibe.Api;

public static class ResultExtensions
{
    private static readonly Dictionary<FailureKind, int> StatusByFailureKind = new()
    {
        [FailureKind.NotFound] = StatusCodes.Status404NotFound,
        [FailureKind.Validation] = StatusCodes.Status400BadRequest,
        [FailureKind.Conflict] = StatusCodes.Status409Conflict,
        [FailureKind.Forbidden] = StatusCodes.Status403Forbidden,
        [FailureKind.Unauthorized] = StatusCodes.Status401Unauthorized,
    };

    public static IActionResult ToActionResult<T>(this Result<T> result, int successStatus = StatusCodes.Status200OK)
    {
        if (result.IsOk)
        {
            return new ObjectResult(result.Value) { StatusCode = successStatus };
        }

        var error = result.Error!;
        return new ObjectResult(new { error = error.Kind, message = error.Message, details = error.Details })
        {
            StatusCode = StatusByFailureKind[error.Kind],
        };
    }
}
