using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;

    public SemesterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SemesterModel?> GetByIdAsync(int id, string? expand = null)
    {
        var entity = await _unitOfWork.Semesters.GetByIdAsync(id, expand);
        return entity == null ? null : MapToModel(entity);
    }

    public async Task<PagedResult<SemesterModel>> GetAllAsync(QueryParameters parameters)
    {
        var searchPredicate = string.IsNullOrWhiteSpace(parameters.Search) ? null :
            (System.Linq.Expressions.Expression<Func<Semester, bool>>)(s =>
                s.SemesterName.Contains(parameters.Search));

        var result = await _unitOfWork.Semesters.GetAllAsync(parameters, searchPredicate);

        return new PagedResult<SemesterModel>
        {
            Items = result.Items.Select(MapToModel).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        };
    }

    public async Task<SemesterModel> CreateAsync(SemesterModel model)
    {
        var entity = MapToEntity(model);
        var created = await _unitOfWork.Semesters.CreateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return MapToModel(created);
    }

    public async Task<SemesterModel?> UpdateAsync(int id, SemesterModel model)
    {
        if (!await _unitOfWork.Semesters.ExistsAsync(id)) return null;
        var entity = MapToEntity(model);
        entity.SemesterId = id;
        var updated = await _unitOfWork.Semesters.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return MapToModel(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _unitOfWork.Semesters.DeleteAsync(id);
        if (result) await _unitOfWork.SaveChangesAsync();
        return result;
    }

    private static SemesterModel MapToModel(Semester entity) => new()
    {
        SemesterId = entity.SemesterId,
        SemesterName = entity.SemesterName,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Courses = entity.Courses?.Any() == true
            ? entity.Courses.Select(c => new CourseModel
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                SemesterId = c.SemesterId,
                SubjectId = c.SubjectId
            }).ToList()
            : null
    };

    private static Semester MapToEntity(SemesterModel model) => new()
    {
        SemesterId = model.SemesterId,
        SemesterName = model.SemesterName,
        StartDate = model.StartDate,
        EndDate = model.EndDate
    };
}
