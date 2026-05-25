namespace PRN232.LMS.Services.Models.Common
{
    public class ResponseModel<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Errors { get; set; }

        public static ResponseModel<T> SuccessResponse(T data, string message = "Request processed successfully")
        {
            return new ResponseModel<T> { Success = true, Message = message, Data = data };
        }

        public static ResponseModel<T> ErrorResponse(string message, object? errors = null)
        {
            return new ResponseModel<T> { Success = false, Message = message, Errors = errors };
        }
    }
}
