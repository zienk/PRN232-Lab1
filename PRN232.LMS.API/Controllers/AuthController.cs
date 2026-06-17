using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Common;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.API.Controllers;

// Controller xử lý Authentication: login, refresh token và revoke token
// Không cần [Authorize] vì đây là public endpoint
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/auth")]
[Produces("application/json", "application/xml")] // Content Negotiation
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập và lấy JWT token
    /// </summary>
    /// <remarks>
    /// Tài khoản mặc định:
    /// - Admin: username=admin, password=Admin@123
    /// - User: username=user1, password=User@123
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ResponseModel<AuthResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseModel<AuthResponseModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // ModelState.IsValid kiểm tra các [Required], [StringLength] đã khai báo trong model
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateValidationErrorResponse());
        }

        var result = await _authService.LoginAsync(request);

        // Trả về 401 nếu login thất bại, 200 nếu thành công
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Làm mới access token bằng refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ResponseModel<AuthResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseModel<AuthResponseModel>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateValidationErrorResponse());
        }

        var result = await _authService.RefreshTokenAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Thu hồi refresh token khi logout hoặc muốn vô hiệu hóa phiên đăng nhập
    /// </summary>
    [HttpPost("revoke")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(CreateValidationErrorResponse());
        }

        var result = await _authService.RevokeTokenAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    private object CreateValidationErrorResponse()
    {
        return new
        {
            success = false,
            message = "Dữ liệu không hợp lệ",
            errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
        };
    }
}
