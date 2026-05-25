namespace PRN232.LMS.Services.Models.Common
{
    public class PaginationMetadataModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
