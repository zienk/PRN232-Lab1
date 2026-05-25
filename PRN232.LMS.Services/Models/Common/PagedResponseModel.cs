using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.Common
{
    public class PagedResponseModel<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<T>? Data { get; set; }
        public PaginationMetadataModel? Pagination { get; set; }
        public object? Errors { get; set; }

        public static PagedResponseModel<T> SuccessResponse(List<T> data, PaginationMetadataModel pagination, string message = "Request processed successfully")
        {
            return new PagedResponseModel<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Pagination = pagination
            };
        }

        public static PagedResponseModel<T> ErrorResponse(string message, object? errors = null)
        {
            return new PagedResponseModel<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
