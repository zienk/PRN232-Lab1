using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

// Interface định nghĩa các thao tác với bảng RefreshToken
public interface IRefreshTokenRepository
{
    // Lấy refresh token từ DB theo chuỗi token
    Task<RefreshToken?> GetByTokenAsync(string token);

    // Tạo mới một refresh token trong DB
    Task<RefreshToken> CreateAsync(RefreshToken refreshToken);

    // Thu hồi (vô hiệu hóa) một refresh token
    Task RevokeAsync(string token);
}
