namespace PRN232.LMS.API.Models.Responses;

/// <summary>
/// Standard API response wrapper ensuring consistent response format across all endpoints.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request processed successfully")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    public static ApiResponse<T> ErrorResponse(string message, object? errors = null)
    {
        return new ApiResponse<T> { Success = false, Message = message, Errors = errors };
    }
}

/// <summary>
/// Paginated list response with pagination metadata.
/// </summary>
public class PaginatedResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<object>? Data { get; set; }
    public PaginationMetadata? Pagination { get; set; }
    public object? Errors { get; set; }
}

/// <summary>
/// Pagination metadata included in list responses.
/// </summary>
public class PaginationMetadata
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
