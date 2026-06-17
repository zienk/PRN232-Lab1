using Microsoft.AspNetCore.Mvc;

namespace PRN232.LMS.API.Extensions;

public static class ControllerRequestExtensions
{
    public static void EchoRequestId(this ControllerBase controller, string? requestId)
    {
        if (!string.IsNullOrWhiteSpace(requestId))
        {
            controller.Response.Headers["X-Request-Id"] = requestId;
        }
    }
}
