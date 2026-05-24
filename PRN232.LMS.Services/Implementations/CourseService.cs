using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CourseModel?> GetByIdAsync(int id, string? expand = null)
    {
        var entity = await _unitOfWork.Courses.GetByIdAsync(id, expand ?? "semester,subject");
        return entity == null ? null : MapToModel(entity);
    }

    public async Task<PagedResult<CourseModel>> GetAllAsync(QueryParameters parameters)
    {
        var searchPredicate = string.IsNullOrWhiteSpace(parameters.Search) ? null :
            (System.Linq.Expressions.Expression<Func<Course, bool>>)(c =>
                c.CourseName.Contains(parameters.Search));

        var result = await _unitOfWork.Courses.GetAllAsync(parameters, searchPredicate);

        return new PagedResult<CourseModel>
        {
            Items = result.Items.Select(MapToModel).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        };
    }

    public async Task<CourseModel> CreateAsync(CourseModel model)
    {
        var entity = MapToEntity(model);
        var created = await _unitOfWork.Courses.CreateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return MapToModel(created);
    }

    public async Task<CourseModel?> UpdateAsync(int id, CourseModel model)
    {
        if (!await _unitOfWork.Courses.ExistsAsync(id)) return null;
        var entity = MapToEntity(model);
        entity.CourseId = id;
        var updated = await _unitOfWork.Courses.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return MapToModel(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _unitOfWork.Courses.DeleteAsync(id);
        if (result) await _unitOfWork.SaveChangesAsync();
        return result;
    }

    private static CourseModel MapToModel(Course entity) => new()
    {
        CourseId = entity.CourseId,
        CourseName = entity.CourseName,
        SemesterId = entity.SemesterId,
        SubjectId = entity.SubjectId,
        Semester = entity.Semester != null ? new SemesterModel
        {
            SemesterId = entity.Semester.SemesterId,
            SemesterName = entity.Semester.SemesterName,
            StartDate = entity.Semester.StartDate,
            EndDate = entity.Semester.EndDate
        } : null,
        Subject = entity.Subject != null ? new SubjectModel
        {
            SubjectId = entity.Subject.SubjectId,
            SubjectCode = entity.Subject.SubjectCode,
            SubjectName = entity.Subject.SubjectName,
            Credit = entity.Subject.Credit
        } : null
    };

    private static Course MapToEntity(CourseModel model) => new()
    {
        CourseId = model.CourseId,
        CourseName = model.CourseName,
        SemesterId = model.SemesterId,
        SubjectId = model.SubjectId
    };
}
