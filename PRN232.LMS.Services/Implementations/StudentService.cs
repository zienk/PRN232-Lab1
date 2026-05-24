using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;

    // Tiêm Unit of Work để quản lý các Repository đồng bộ
    public StudentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lấy chi tiết học sinh theo ID và nạp các bảng liên quan nếu có
    public async Task<StudentModel?> GetByIdAsync(int id, string? expand = null)
    {
        var entity = await _unitOfWork.Students.GetByIdAsync(id, expand);
        return entity == null ? null : MapToModel(entity);
    }

    // Lấy danh sách học sinh có phân trang, sắp xếp và tìm kiếm theo tên hoặc email
    public async Task<PagedResult<StudentModel>> GetAllAsync(QueryParameters parameters)
    {
        // Xây dựng điều kiện tìm kiếm động (FullName hoặc Email chứa từ khóa search)
        var searchPredicate = string.IsNullOrWhiteSpace(parameters.Search) ? null :
            (System.Linq.Expressions.Expression<Func<Student, bool>>)(s =>
                s.FullName.Contains(parameters.Search) ||
                s.Email.Contains(parameters.Search));

        // Gọi repo để thực hiện truy vấn phân trang, tìm kiếm và sắp xếp động
        var result = await _unitOfWork.Students.GetAllAsync(parameters, searchPredicate);

        return new PagedResult<StudentModel>
        {
            Items = result.Items.Select(MapToModel).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        };
    }

    // Tạo mới học sinh và lưu vào cơ sở dữ liệu
    public async Task<StudentModel> CreateAsync(StudentModel model)
    {
        var entity = MapToEntity(model);
        var created = await _unitOfWork.Students.CreateAsync(entity);
        await _unitOfWork.SaveChangesAsync(); // Lưu thay đổi xuống Database
        return MapToModel(created);
    }

    // Cập nhật thông tin học sinh theo ID
    public async Task<StudentModel?> UpdateAsync(int id, StudentModel model)
    {
        // Trả về null nếu học sinh không tồn tại
        if (!await _unitOfWork.Students.ExistsAsync(id)) return null;
        
        var entity = MapToEntity(model);
        entity.StudentId = id;
        
        var updated = await _unitOfWork.Students.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync(); // Lưu thay đổi
        return MapToModel(updated);
    }

    // Xóa học sinh theo ID
    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _unitOfWork.Students.DeleteAsync(id);
        if (result) await _unitOfWork.SaveChangesAsync(); // Chỉ lưu nếu xóa thành công
        return result;
    }

    // Chuyển đổi Entity sang Business Model
    private static StudentModel MapToModel(Student entity) => new()
    {
        StudentId = entity.StudentId,
        FullName = entity.FullName,
        Email = entity.Email,
        DateOfBirth = entity.DateOfBirth,
        Enrollments = entity.Enrollments?.Any() == true
            ? entity.Enrollments.Select(e => new EnrollmentModel
            {
                EnrollmentId = e.EnrollmentId,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                EnrollDate = e.EnrollDate,
                Status = e.Status
            }).ToList()
            : null
    };

    // Chuyển đổi Business Model sang Entity
    private static Student MapToEntity(StudentModel model) => new()
    {
        StudentId = model.StudentId,
        FullName = model.FullName,
        Email = model.Email,
        DateOfBirth = model.DateOfBirth
    };
}
