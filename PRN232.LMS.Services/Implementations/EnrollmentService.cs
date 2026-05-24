using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;

    // Tiêm Unit of Work để phối hợp các tác vụ dữ liệu
    public EnrollmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Lấy chi tiết lượt đăng ký học theo ID (nạp kèm student và course mặc định)
    public async Task<EnrollmentModel?> GetByIdAsync(int id, string? expand = null)
    {
        var entity = await _unitOfWork.Enrollments.GetByIdAsync(id, expand ?? "student,course");
        return entity == null ? null : MapToModel(entity);
    }

    // Lấy danh sách lượt đăng ký học tập có phân trang, sắp xếp và tìm kiếm theo trạng thái (Status)
    public async Task<PagedResult<EnrollmentModel>> GetAllAsync(QueryParameters parameters)
    {
        // Tạo điều kiện lọc theo trạng thái nếu có từ khóa search
        var searchPredicate = string.IsNullOrWhiteSpace(parameters.Search) ? null :
            (System.Linq.Expressions.Expression<Func<Enrollment, bool>>)(e =>
                e.Status.Contains(parameters.Search));

        // Gọi repo để thực thi truy vấn động
        var result = await _unitOfWork.Enrollments.GetAllAsync(parameters, searchPredicate);

        return new PagedResult<EnrollmentModel>
        {
            Items = result.Items.Select(MapToModel).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems
        };
    }

    // Tạo mới một lượt đăng ký học tập
    public async Task<EnrollmentModel> CreateAsync(EnrollmentModel model)
    {
        var entity = MapToEntity(model);
        var created = await _unitOfWork.Enrollments.CreateAsync(entity);
        await _unitOfWork.SaveChangesAsync(); // Lưu thay đổi
        return MapToModel(created);
    }

    // Cập nhật lượt đăng ký học tập
    public async Task<EnrollmentModel?> UpdateAsync(int id, EnrollmentModel model)
    {
        // Trả về null nếu lượt đăng ký cần cập nhật không tồn tại
        if (!await _unitOfWork.Enrollments.ExistsAsync(id)) return null;
        
        var entity = MapToEntity(model);
        entity.EnrollmentId = id;
        
        var updated = await _unitOfWork.Enrollments.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync(); // Lưu thay đổi
        return MapToModel(updated);
    }

    // Xóa/Hủy một lượt đăng ký học tập theo ID
    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _unitOfWork.Enrollments.DeleteAsync(id);
        if (result) await _unitOfWork.SaveChangesAsync(); // Chỉ lưu khi xóa thành công
        return result;
    }

    // Chuyển đổi Entity sang Business Model
    private static EnrollmentModel MapToModel(Enrollment entity) => new()
    {
        EnrollmentId = entity.EnrollmentId,
        StudentId = entity.StudentId,
        CourseId = entity.CourseId,
        EnrollDate = entity.EnrollDate,
        Status = entity.Status,
        Student = entity.Student != null ? new StudentModel
        {
            StudentId = entity.Student.StudentId,
            FullName = entity.Student.FullName,
            Email = entity.Student.Email,
            DateOfBirth = entity.Student.DateOfBirth
        } : null,
        Course = entity.Course != null ? new CourseModel
        {
            CourseId = entity.Course.CourseId,
            CourseName = entity.Course.CourseName,
            SemesterId = entity.Course.SemesterId,
            SubjectId = entity.Course.SubjectId
        } : null
    };

    // Chuyển đổi Business Model sang Entity
    private static Enrollment MapToEntity(EnrollmentModel model) => new()
    {
        EnrollmentId = model.EnrollmentId,
        StudentId = model.StudentId,
        CourseId = model.CourseId,
        EnrollDate = model.EnrollDate,
        Status = model.Status
    };
}
