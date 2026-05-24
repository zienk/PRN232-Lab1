using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ICourseService
{
    Task<CourseModel?> GetByIdAsync(int id, string? expand = null);
    Task<PagedResult<CourseModel>> GetAllAsync(QueryParameters parameters);
    Task<CourseModel> CreateAsync(CourseModel model);
    Task<CourseModel?> UpdateAsync(int id, CourseModel model);
    Task<bool> DeleteAsync(int id);
}
