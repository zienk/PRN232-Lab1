using System.Linq.Expressions;
using PRN232.LMS.Repositories.Common;

namespace PRN232.LMS.Repositories.Interfaces;

/// <summary>
/// Generic repository interface for common CRUD and query operations.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, string? expand = null);
    Task<PagedResult<T>> GetAllAsync(QueryParameters parameters, Expression<Func<T, bool>>? searchPredicate = null);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
