using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ISemesterService
{
    Task<SemesterModel?> GetByIdAsync(int id, string? expand = null);
    Task<PagedResult<SemesterModel>> GetAllAsync(QueryParameters parameters);
    Task<SemesterModel> CreateAsync(SemesterModel model);
    Task<SemesterModel?> UpdateAsync(int id, SemesterModel model);
    Task<bool> DeleteAsync(int id);
}
