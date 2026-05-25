using System.Threading.Tasks;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces
{
    public interface ICourseService
    {
        Task<PagedResponseModel<object>> GetCoursesAsync(string? search, string? sort, int page, int size, string? fields, string? expand);
        Task<ResponseModel<CourseResponseModel>> GetCourseByIdAsync(int id);
        Task<ResponseModel<CourseResponseModel>> CreateCourseAsync(CourseRequestModel model);
        Task<ResponseModel<CourseResponseModel>> UpdateCourseAsync(int id, CourseRequestModel model);
        Task<ResponseModel<bool>> DeleteCourseAsync(int id);
    }
}
