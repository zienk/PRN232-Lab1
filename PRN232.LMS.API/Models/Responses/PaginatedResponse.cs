namespace PRN232.LMS.API.Models.Responses;

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
