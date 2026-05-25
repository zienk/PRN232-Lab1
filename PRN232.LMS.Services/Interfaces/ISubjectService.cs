using System.Threading.Tasks;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.Services.Interfaces
{
    public interface ISubjectService
    {
        Task<PagedResponseModel<object>> GetSubjectsAsync(string? search, string? sort, int page, int size, string? fields, string? expand);
        Task<ResponseModel<SubjectResponseModel>> GetSubjectByIdAsync(int id);
        Task<ResponseModel<SubjectResponseModel>> CreateSubjectAsync(SubjectRequestModel model);
        Task<ResponseModel<SubjectResponseModel>> UpdateSubjectAsync(int id, SubjectRequestModel model);
        Task<ResponseModel<bool>> DeleteSubjectAsync(int id);
    }
}
