using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.API.Controllers;

// Controller xử lý Authentication: login và refresh token
// Không cần [Authorize] vì đây là public endpoint
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
[Route("api/auth")] // Route không có version cho backward compatibility
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
            return BadRequest(new
            {
                success = false,
                message = "Dữ liệu không hợp lệ",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
            });
        }

        var result = await _authService.LoginAsync(request);

        // Trả về 401 nếu login thất bại, 200 nếu thành công
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Làm mới access token bằng refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ResponseModel<AuthResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseModel<AuthResponseModel>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Dữ liệu không hợp lệ",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
            });
        }

        var result = await _authService.RefreshTokenAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }
}
