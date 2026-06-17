using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces;

// Interface định nghĩa các chức năng Authentication
public interface IAuthService
{
    // Đăng nhập: kiểm tra username/password, trả về JWT token nếu hợp lệ
    Task<ResponseModel<AuthResponseModel>> LoginAsync(LoginRequest request);

    // Làm mới access token bằng refresh token
    Task<ResponseModel<AuthResponseModel>> RefreshTokenAsync(RefreshTokenRequest request);

    // Thu hồi refresh token khi user logout hoặc muốn vô hiệu hóa phiên đăng nhập
    Task<ResponseModel<bool>> RevokeTokenAsync(RefreshTokenRequest request);
}
