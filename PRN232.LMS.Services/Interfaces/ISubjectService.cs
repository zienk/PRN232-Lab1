using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface ISubjectService
{
    Task<SubjectModel?> GetByIdAsync(int id, string? expand = null);
    Task<PagedResult<SubjectModel>> GetAllAsync(QueryParameters parameters);
    Task<SubjectModel> CreateAsync(SubjectModel model);
    Task<SubjectModel?> UpdateAsync(int id, SubjectModel model);
    Task<bool> DeleteAsync(int id);
}
