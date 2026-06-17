using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.API.Controllers;

// Controller nhỏ phục vụ demo phân quyền Lab 2.
// Endpoint này là GET Admin-only để test 403 dễ hơn, không cần chuẩn bị request body.
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/admin")]
[Produces("application/json", "application/xml")]
[Authorize(Policy = "Lab2Admin")]
public class AdminController : ControllerBase
{
    /// <summary>
    /// Kiểm tra quyền Admin của access token hiện tại
    /// </summary>
    [HttpGet("permission-check")]
    [ProducesResponseType(typeof(ResponseModel<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult PermissionCheck()
    {
        var username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "unknown";

        var data = new
        {
            username,
            role,
            canAccessAdminApi = true
        };

        return Ok(ResponseModel<object>.SuccessResponse(
            data,
            "Token hợp lệ và có quyền Admin"));
    }
}
