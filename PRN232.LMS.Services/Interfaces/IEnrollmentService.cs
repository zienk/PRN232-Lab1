using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Services.BusinessModels;

namespace PRN232.LMS.Services.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentModel?> GetByIdAsync(int id, string? expand = null);
    Task<PagedResult<EnrollmentModel>> GetAllAsync(QueryParameters parameters);
    Task<EnrollmentModel> CreateAsync(EnrollmentModel model);
    Task<EnrollmentModel?> UpdateAsync(int id, EnrollmentModel model);
    Task<bool> DeleteAsync(int id);
}
