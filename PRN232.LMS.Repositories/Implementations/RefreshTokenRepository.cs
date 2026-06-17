using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

// Implementation của IRefreshTokenRepository
// Xử lý việc lưu, lấy và thu hồi refresh token trong DB
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly LmsDbContext _context;

    public RefreshTokenRepository(LmsDbContext context)
    {
        _context = context;
    }

    // Tìm refresh token theo chuỗi token
    // Include thêm User để lấy thông tin phân quyền khi cần
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(r => r.User) // Nạp luôn thông tin User liên quan
            .FirstOrDefaultAsync(r => r.Token == token);
    }

    // Lưu refresh token mới vào DB
    public async Task<RefreshToken> CreateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    // Thu hồi refresh token: đánh dấu IsRevoked = true
    // Không xóa khỏi DB để giữ audit trail
    public async Task RevokeAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            await _context.SaveChangesAsync();
        }
    }
}
