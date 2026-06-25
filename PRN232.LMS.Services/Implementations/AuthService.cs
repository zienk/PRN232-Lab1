using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Common;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;

namespace PRN232.LMS.Services.Implementations;

// AuthService xử lý toàn bộ logic Authentication
// Gồm: login, tạo JWT, refresh token, revoke token
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    // Inject IUnitOfWork (để truy cập DB) và IConfiguration (để đọc JWT settings)
    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    // Đăng nhập: kiểm tra username/password, tạo JWT token
    public async Task<ResponseModel<AuthResponseModel>> LoginAsync(LoginRequest request)
    {
        // Bước 1: Tìm user theo username trong DB
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username);

        // Bước 2: Kiểm tra user có tồn tại không
        if (user == null)
        {
            // Trả về lỗi chung chung để tránh lộ thông tin (không nói rõ user không tồn tại)
            return ResponseModel<AuthResponseModel>.ErrorResponse("Username hoặc password không đúng");
        }

        // Bước 3: Kiểm tra password bằng BCrypt
        // BCrypt.Verify so sánh password plain-text với hash trong DB
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return ResponseModel<AuthResponseModel>.ErrorResponse("Username hoặc password không đúng");
        }

        // Bước 4: Tạo JWT access token
        var accessToken = GenerateJwtToken(user);

        // Bước 5: Tạo refresh token (chuỗi ngẫu nhiên)
        var refreshTokenString = GenerateRefreshToken();

        // Bước 6: Lưu refresh token vào DB
        var refreshToken = new RefreshToken
        {
            UserId = user.UserId,
            Token = refreshTokenString,
            // Refresh token có hiệu lực trong 7 ngày
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        await _unitOfWork.RefreshTokens.CreateAsync(refreshToken);

        // Bước 7: Lấy thời gian sống của access token từ config
        var expiresInMinutes = GetAccessTokenExpirationMinutes();

        // Bước 8: Trả về response chứa tokens
        var response = new AuthResponseModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresIn = expiresInMinutes * 60 // Chuyển từ phút sang giây
        };

        return ResponseModel<AuthResponseModel>.SuccessResponse(response, "Đăng nhập thành công");
    }

    // Làm mới access token bằng refresh token
    public async Task<ResponseModel<AuthResponseModel>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Bước 1: Tìm refresh token trong DB (kèm theo thông tin User)
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

        // Bước 2: Kiểm tra token có hợp lệ không
        if (storedToken == null)
        {
            return ResponseModel<AuthResponseModel>.ErrorResponse("Refresh token không hợp lệ");
        }

        // Kiểm tra token đã bị revoke chưa
        if (storedToken.IsRevoked)
        {
            return ResponseModel<AuthResponseModel>.ErrorResponse("Refresh token đã bị thu hồi");
        }

        // Kiểm tra token còn hiệu lực không
        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return ResponseModel<AuthResponseModel>.ErrorResponse("Refresh token đã hết hạn");
        }

        // Bước 3: Thu hồi refresh token cũ (mỗi refresh chỉ dùng 1 lần)
        await _unitOfWork.RefreshTokens.RevokeAsync(request.RefreshToken);

        // Bước 4: Tạo access token mới và refresh token mới
        var user = storedToken.User;
        var newAccessToken = GenerateJwtToken(user);
        var newRefreshTokenString = GenerateRefreshToken();

        // Bước 5: Lưu refresh token mới vào DB
        var newRefreshToken = new RefreshToken
        {
            UserId = user.UserId,
            Token = newRefreshTokenString,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        await _unitOfWork.RefreshTokens.CreateAsync(newRefreshToken);

        var expiresInMinutes = GetAccessTokenExpirationMinutes();

        var response = new AuthResponseModel
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenString,
            ExpiresIn = expiresInMinutes * 60
        };

        return ResponseModel<AuthResponseModel>.SuccessResponse(response, "Làm mới token thành công");
    }

    // Thu hồi refresh token để client không thể dùng lại token đó
    public async Task<ResponseModel<bool>> RevokeTokenAsync(RefreshTokenRequest request)
    {
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken);

        if (storedToken == null)
        {
            return ResponseModel<bool>.ErrorResponse("Refresh token không hợp lệ");
        }

        if (storedToken.IsRevoked)
        {
            return ResponseModel<bool>.SuccessResponse(true, "Refresh token đã được thu hồi trước đó");
        }

        await _unitOfWork.RefreshTokens.RevokeAsync(request.RefreshToken);

        return ResponseModel<bool>.SuccessResponse(true, "Thu hồi refresh token thành công");
    }

    // Hàm private tạo JWT access token
    // JWT gồm 3 phần: header.payload.signature
    private string GenerateJwtToken(User user)
    {
        // Đọc secret key từ appsettings.json (hoặc environment variable)
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey chưa được cấu hình");

        // Tạo signing key từ secret
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Claims là thông tin được nhúng vào token
        // Client có thể đọc claims nhưng không thể sửa (vì có chữ ký)
        var claims = new[]
        {
            // ClaimTypes.NameIdentifier: ID của user
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            // ClaimTypes.Name: tên đăng nhập
            new Claim(ClaimTypes.Name, user.Username),
            // ClaimTypes.Role: role dùng cho [Authorize(Roles = "Admin")]
            new Claim(ClaimTypes.Role, user.Role),
            // jti: JWT ID - định danh duy nhất cho mỗi token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Thời gian sống của token (đọc từ config)
        var expiresInMinutes = GetAccessTokenExpirationMinutes();

        // Tạo token descriptor
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],       // Ai cấp token
            audience: _configuration["Jwt:Audience"],   // Token dùng cho ai
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials
        );

        // Serialize token thành chuỗi "header.payload.signature"
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Hàm private tạo refresh token (chuỗi ngẫu nhiên an toàn)
    // Dùng RandomNumberGenerator để đảm bảo entropy cao
    private static string GenerateRefreshToken()
    {
        // Tạo 64 bytes ngẫu nhiên và encode thành Base64
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private int GetAccessTokenExpirationMinutes()
    {
        var configuredValue = _configuration["Jwt:AccessTokenExpirationMinutes"]
            ?? _configuration["Jwt:ExpiresInMinutes"];

        return int.TryParse(configuredValue, out var minutes) && minutes > 0
            ? minutes
            : 60;
    }

    private int GetRefreshTokenExpirationDays()
    {
        var configuredValue = _configuration["Jwt:RefreshTokenExpirationDays"];

        return int.TryParse(configuredValue, out var days) && days > 0
            ? days
            : 7;
    }
}
