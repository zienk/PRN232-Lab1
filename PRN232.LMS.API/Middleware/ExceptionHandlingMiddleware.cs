using System.Net;
using System.Text.Json;

namespace PRN232.LMS.API.Middleware;

// Middleware bắt tất cả exception chưa được xử lý trong toàn ứng dụng
// Đây là "lưới an toàn" cuối cùng để tránh lộ lỗi nội bộ ra client
public class ExceptionHandlingMiddleware
{
    // _next: đại diện cho middleware tiếp theo trong pipeline
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Hàm này được gọi mỗi khi có request đến
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Gọi middleware tiếp theo trong pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Ghi log lỗi chi tiết ở server (không gửi cho client)
            _logger.LogError(ex, "Lỗi không được xử lý: {Message}", ex.Message);

            // Xử lý exception và trả về response nhất quán
            await HandleExceptionAsync(context, ex);
        }
    }

    // Hàm private tạo error response theo format chuẩn
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Xác định HTTP status code dựa trên loại exception
        var statusCode = exception switch
        {
            // 404: không tìm thấy tài nguyên
            KeyNotFoundException => HttpStatusCode.NotFound,
            // 400: request không hợp lệ
            ArgumentException => HttpStatusCode.BadRequest,
            // 401: chưa xác thực
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            // 500: lỗi server mặc định
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Tạo error response theo format chuẩn
        // KHÔNG trả về ex.Message để tránh lộ thông tin nội bộ (tên class, stack trace...)
        var response = new
        {
            success = false,
            message = statusCode == HttpStatusCode.InternalServerError
                ? "Đã xảy ra lỗi nội bộ. Vui lòng thử lại sau."
                : exception.Message,
            errors = (object?)null // Không có danh sách lỗi cụ thể
        };

        // Serialize thành JSON và ghi vào response
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase // camelCase format
        });

        await context.Response.WriteAsync(json);
    }
}
