using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

// Entity lưu Refresh Token để cấp lại Access Token khi hết hạn
// Mỗi lần login sẽ tạo một refresh token mới và lưu vào bảng này
[Table("RefreshToken")]
public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    // Khóa ngoại tham chiếu đến bảng User
    public int UserId { get; set; }

    // Chuỗi token ngẫu nhiên (random GUID/base64)
    [Required]
    [MaxLength(500)]
    public string Token { get; set; } = string.Empty;

    // Thời điểm token hết hạn
    public DateTime ExpiresAt { get; set; }

    // Thời điểm tạo token
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Đánh dấu token đã bị thu hồi (revoke) hay chưa
    public bool IsRevoked { get; set; } = false;

    // Navigation: liên kết về user sở hữu token này
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
