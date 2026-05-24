using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public SubjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SubjectModel?> GetByIdAsync(int id, string? expand = null)
    {
        var entity = await _unitOfWork.Subjects.GetByIdAsync(id, expand);
        return entity == null ? null : MapToModel(entity);
    }

    public async Task<PagedResult<SubjectModel>> GetAllAsync(QueryParameters parameters)
    {
        var searchPredicate = string.IsNullOrWhiteSpace(parameters.Search) ? null :
            (System.Linq.Expressions.Expression<Func<Subject, bool>>)(s =>
                s.SubjectName.Contains(parameters.Search) ||
                s.SubjectCode.Contains(parameters.Search));

        var result = await _unitOfWork.Subjects.GetAllAsync(parameters, searchPredicate);

        return new PagedResult<SubjectModel>
        {
            Items = result.Items.Select(MapToModel).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        };
    }

    public async Task<SubjectModel> CreateAsync(SubjectModel model)
    {
        var entity = MapToEntity(model);
        var created = await _unitOfWork.Subjects.CreateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return MapToModel(created);
    }

    public async Task<SubjectModel?> UpdateAsync(int id, SubjectModel model)
    {
        if (!await _unitOfWork.Subjects.ExistsAsync(id)) return null;
        var entity = MapToEntity(model);
        entity.SubjectId = id;
        var updated = await _unitOfWork.Subjects.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return MapToModel(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _unitOfWork.Subjects.DeleteAsync(id);
        if (result) await _unitOfWork.SaveChangesAsync();
        return result;
    }

    private static SubjectModel MapToModel(Subject entity) => new()
    {
        SubjectId = entity.SubjectId,
        SubjectCode = entity.SubjectCode,
        SubjectName = entity.SubjectName,
        Credit = entity.Credit,
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

    private static Subject MapToEntity(SubjectModel model) => new()
    {
        SubjectId = model.SubjectId,
        SubjectCode = model.SubjectCode,
        SubjectName = model.SubjectName,
        Credit = model.Credit
    };
}
