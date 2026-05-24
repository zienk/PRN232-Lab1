using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

// Lớp Generic Repository triển khai các tác vụ truy xuất dữ liệu động phổ biến
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly LmsDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(LmsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // Lấy một thực thể theo khóa chính ID (hỗ trợ nạp bảng liên quan qua tham số expand)
    public virtual async Task<T?> GetByIdAsync(int id, string? expand = null)
    {
        IQueryable<T> query = _dbSet;
        query = ApplyExpansion(query, expand);
        
        // Tự động tìm thuộc tính ID của thực thể bằng Reflection
        var keyProperty = typeof(T).GetProperties()
            .FirstOrDefault(p => p.Name.EndsWith("Id") && p.Name.Length > 2)
            ?? typeof(T).GetProperties().First();

        // Xây dựng biểu thức lambda x => x.Id == id động
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, keyProperty);
        var constant = Expression.Constant(id);
        var equals = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

        return await query.FirstOrDefaultAsync(lambda);
    }

    // Lấy danh sách thực thể có hỗ trợ tìm kiếm, sắp xếp động, phân trang và nạp bảng liên quan
    public virtual async Task<PagedResult<T>> GetAllAsync(
        QueryParameters parameters,
        Expression<Func<T, bool>>? searchPredicate = null)
    {
        IQueryable<T> query = _dbSet;

        // Áp dụng nạp chồng các bảng liên quan (Eager Loading)
        query = ApplyExpansion(query, parameters.Expand);

        // Áp dụng bộ lọc tìm kiếm
        if (searchPredicate != null)
        {
            query = query.Where(searchPredicate);
        }

        // Lấy tổng số dòng dữ liệu trước khi phân trang để trả về metadata
        var totalItems = await query.CountAsync();

        // Áp dụng sắp xếp động (Sort)
        query = ApplySorting(query, parameters.Sort);

        // Áp dụng phân trang (Paging) bằng cách Skip và Take
        query = query.Skip((parameters.Page - 1) * parameters.Size).Take(parameters.Size);

        var items = await query.ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            Page = parameters.Page,
            PageSize = parameters.Size,
            TotalItems = totalItems
        };
    }

    // Thêm mới một thực thể vào DB
    public virtual async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    // Cập nhật thông tin thực thể
    public virtual Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    // Xóa một thực thể theo ID
    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;
        _dbSet.Remove(entity);
        return true;
    }

    // Kiểm tra sự tồn tại của thực thể theo ID
    public virtual async Task<bool> ExistsAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity != null;
    }

    // Phương thức sắp xếp động bằng Expression Tree dựa vào tham số sort (ví dụ: "fullName,-dateOfBirth")
    protected IQueryable<T> ApplySorting(IQueryable<T> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return query;

        var sortFields = sort.Split(',', StringSplitOptions.RemoveEmptyEntries);
        bool isFirst = true;

        foreach (var field in sortFields)
        {
            var trimmed = field.Trim();
            var descending = trimmed.StartsWith('-');
            var propertyName = descending ? trimmed[1..] : trimmed;

            // Tìm thuộc tính trong Entity trùng khớp tên (không phân biệt hoa thường)
            var property = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));

            if (property == null) continue;

            // Xây dựng Expression Tree truy cập thuộc tính (x => x.PropertyName)
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            // Xác định hàm sắp xếp tương ứng (OrderBy / ThenBy)
            string methodName;
            if (isFirst)
            {
                methodName = descending ? "OrderByDescending" : "OrderBy";
                isFirst = false;
            }
            else
            {
                methodName = descending ? "ThenByDescending" : "ThenBy";
            }

            // Gọi động phương thức OrderBy/ThenBy bằng Reflection
            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExpression));

            query = query.Provider.CreateQuery<T>(resultExpression);
        }

        return query;
    }

    // Phương thức nạp chồng (Include) các bảng liên quan động dựa vào tham số expand (ví dụ: "student,course")
    protected IQueryable<T> ApplyExpansion(IQueryable<T> query, string? expand)
    {
        if (string.IsNullOrWhiteSpace(expand)) return query;

        var expansions = expand.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var expansion in expansions)
        {
            var trimmed = expansion.Trim();
            // Đưa ký tự đầu lên chữ hoa PascalCase để khớp với thuộc tính liên kết trong EF Core
            var navigationName = char.ToUpper(trimmed[0]) + trimmed[1..];
            
            var navProperty = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name.Equals(navigationName, StringComparison.OrdinalIgnoreCase));

            if (navProperty != null)
            {
                query = query.Include(navigationName); // Thực hiện nạp dữ liệu liên quan
            }
        }

        return query;
    }
}
