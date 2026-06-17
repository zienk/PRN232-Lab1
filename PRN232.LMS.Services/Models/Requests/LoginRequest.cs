using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Services.Models.Requests;

// Model nhận dữ liệu đăng nhập từ client
public class LoginRequest
{
    // Tên đăng nhập - bắt buộc nhập, tối đa 50 ký tự
    [Required(ErrorMessage = "Username là bắt buộc")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username phải từ 3 đến 50 ký tự")]
    public string Username { get; set; } = string.Empty;

    // Mật khẩu - bắt buộc nhập, tối đa 100 ký tự
    [Required(ErrorMessage = "Password là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password phải từ 6 đến 100 ký tự")]
    public string Password { get; set; } = string.Empty;
}
