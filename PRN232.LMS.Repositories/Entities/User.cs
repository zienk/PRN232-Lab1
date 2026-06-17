using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

// Entity đại diện cho người dùng trong hệ thống
// Dùng để xác thực (Authentication) và phân quyền (Authorization)
[Table("User")]
public class User
{
    [Key]
    public int UserId { get; set; }

    // Tên đăng nhập - không được trùng nhau
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    // Mật khẩu đã được hash bằng BCrypt - KHÔNG lưu plain text
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    // Role của user: "Admin" hoặc "User"
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = "User";

    // Navigation: một user có thể có nhiều refresh token
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
