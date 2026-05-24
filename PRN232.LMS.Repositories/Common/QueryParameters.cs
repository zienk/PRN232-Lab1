namespace PRN232.LMS.Repositories.Common;

/// <summary>
/// Common query parameters for list endpoints supporting search, sort, paging, field selection, and expansion.
/// </summary>
public class QueryParameters
{
    private int _pageSize = 10;
    private int _page = 1;

    /// <summary>Search keyword to filter results.</summary>
    public string? Search { get; set; }

    /// <summary>Comma-separated sort fields. Prefix with '-' for descending order. Example: "fullName,-dateOfBirth"</summary>
    public string? Sort { get; set; }

    /// <summary>Page number (1-based).</summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>Number of items per page.</summary>
    public int Size
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : (value > 100 ? 100 : value);
    }

    /// <summary>Comma-separated list of fields to include in the response. Example: "studentId,fullName,email"</summary>
    public string? Fields { get; set; }

    /// <summary>Comma-separated list of related entities to include. Example: "student,course"</summary>
    public string? Expand { get; set; }
}
