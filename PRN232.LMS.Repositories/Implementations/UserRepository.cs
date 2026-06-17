using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

// Implementation của IUserRepository
// Thực hiện query trực tiếp vào DB để tìm User
public class UserRepository : IUserRepository
{
    private readonly LmsDbContext _context;

    public UserRepository(LmsDbContext context)
    {
        _context = context;
    }

    // Tìm user theo username, trả về null nếu không tìm thấy
    // Dùng khi kiểm tra đăng nhập
    public async Task<User?> GetByUsernameAsync(string username)
    {
        // StringComparison.OrdinalIgnoreCase: tìm kiếm không phân biệt hoa thường
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    // Tìm user theo ID
    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }
}
