namespace PRN232.LMS.API.Models.Responses;

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
