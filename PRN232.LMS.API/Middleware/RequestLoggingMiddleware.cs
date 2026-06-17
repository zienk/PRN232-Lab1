using System.Diagnostics;

namespace PRN232.LMS.API.Middleware;

// Middleware ghi log thông tin của mỗi HTTP request
// Lab 2 yêu cầu log: path, method, execution time, status code
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    // Hàm được gọi cho mỗi request
    public async Task InvokeAsync(HttpContext context)
    {
        // Dùng Stopwatch để đo thời gian xử lý request
        var stopwatch = Stopwatch.StartNew();

        // Ghi log request đến
        var requestPath = context.Request.Path;
        var httpMethod = context.Request.Method;

        _logger.LogInformation(
            ">>> REQUEST: [{Method}] {Path} | RequestId: {RequestId}",
            httpMethod,
            requestPath,
            context.TraceIdentifier
        );

        // Xử lý request (gọi middleware tiếp theo)
        await _next(context);

        // Dừng đo thời gian sau khi request hoàn thành
        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        // Ghi log response - bao gồm status code và thời gian xử lý
        var statusCode = context.Response.StatusCode;

        _logger.LogInformation(
            "<<< RESPONSE: [{Method}] {Path} => {StatusCode} | Thời gian: {ElapsedMs}ms",
            httpMethod,
            requestPath,
            statusCode,
            elapsedMs
        );
    }
}
