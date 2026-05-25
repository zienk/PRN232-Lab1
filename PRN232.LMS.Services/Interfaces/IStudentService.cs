using System.Threading.Tasks;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces
{
    public interface IStudentService
    {
        Task<PagedResponseModel<object>> GetStudentsAsync(string? search, string? sort, int page, int size, string? fields, string? expand);
        Task<ResponseModel<StudentResponseModel>> GetStudentByIdAsync(int id);
        Task<ResponseModel<StudentResponseModel>> CreateStudentAsync(StudentRequestModel model);
        Task<ResponseModel<StudentResponseModel>> UpdateStudentAsync(int id, StudentRequestModel model);
        Task<ResponseModel<bool>> DeleteStudentAsync(int id);
    }
}
