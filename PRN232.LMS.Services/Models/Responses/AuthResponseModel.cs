namespace PRN232.LMS.Services.Models.Responses;

// Model trả về thông tin token sau khi login thành công
public class AuthResponseModel
{
    // JWT access token - dùng để gọi các API được bảo vệ
    public string AccessToken { get; set; } = string.Empty;

    // Refresh token - dùng để lấy access token mới khi hết hạn
    public string RefreshToken { get; set; } = string.Empty;

    // Thời gian sống của access token (tính bằng giây)
    // Ví dụ: 3600 = 1 giờ
    public int ExpiresIn { get; set; }

    // Tên đăng nhập để client biết đang đăng nhập với user nào
    public string Username { get; set; } = string.Empty;

    // Role của user để client hiển thị đúng menu/quyền
    public string Role { get; set; } = string.Empty;
}
