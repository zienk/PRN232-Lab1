using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IStudentService
{
    Task<StudentModel?> GetByIdAsync(int id, string? expand = null);
    Task<PagedResult<StudentModel>> GetAllAsync(QueryParameters parameters);
    Task<StudentModel> CreateAsync(StudentModel model);
    Task<StudentModel?> UpdateAsync(int id, StudentModel model);
    Task<bool> DeleteAsync(int id);
}
