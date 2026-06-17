using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Services.Models.Requests;

// Model nhận token khi client muốn làm mới access token
public class RefreshTokenRequest
{
    // Refresh token cũ - bắt buộc phải có
    [Required(ErrorMessage = "Refresh token là bắt buộc")]
    public string RefreshToken { get; set; } = string.Empty;
}
