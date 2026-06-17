using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

// Interface định nghĩa các thao tác với bảng User
public interface IUserRepository
{
    // Tìm user theo username (dùng khi login)
    Task<User?> GetByUsernameAsync(string username);

    // Tìm user theo ID
    Task<User?> GetByIdAsync(int userId);
}
