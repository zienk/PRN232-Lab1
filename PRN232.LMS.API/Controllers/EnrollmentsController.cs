using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Helpers;
using PRN232.LMS.API.Models.Requests;
using PRN232.LMS.API.Models.Responses;
using PRN232.LMS.Repositories.Common;
using PRN232.LMS.Services.BusinessModels;
using PRN232.LMS.Services.Interfaces;

namespace PRN232.LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;
    
    // Tiêm dependency EnrollmentService vào controller
    public EnrollmentsController(IEnrollmentService service) => _service = service;

    /// <summary>
    /// Lấy danh sách đăng ký học tập có hỗ trợ tìm kiếm, sắp xếp, phân trang, lọc trường và mở rộng dữ liệu.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters parameters)
    {
        // Gọi service lấy danh sách đăng ký học đã phân trang
        var result = await _service.GetAllAsync(parameters);
        
        // Map từ Business Model sang Response Model
        var responses = result.Items.Select(MapToResponse).ToList();
        
        // Lọc các trường dữ liệu động nếu có yêu cầu fields
        var data = FieldSelector.SelectFieldsList(responses, parameters.Fields);
        
        return Ok(new PaginatedResponse<EnrollmentResponse>
        {
            Success = true, 
            Message = "Request processed successfully", 
            Data = data,
            Pagination = new PaginationMetadata 
            { 
                Page = result.Page, 
                PageSize = result.PageSize, 
                TotalItems = result.TotalItems, 
                TotalPages = result.TotalPages 
            }
        });
    }

    /// <summary>
    /// Lấy chi tiết thông tin đăng ký học tập theo ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        // Lấy thông tin đăng ký học tập chi tiết từ service
        var model = await _service.GetByIdAsync(id, expand);
        
        // Trả về lỗi 404 nếu không tìm thấy đăng ký học
        if (model == null) return NotFound(ApiResponse<object>.ErrorResponse($"Enrollment with id {id} not found"));
        
        return Ok(ApiResponse<EnrollmentResponse>.SuccessResponse(MapToResponse(model)));
    }

    /// <summary>
    /// Đăng ký môn học mới cho học sinh.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
    {
        // Kiểm tra tính hợp lệ của dữ liệu đầu vào
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", ModelState));
        
        var model = new EnrollmentModel { StudentId = request.StudentId, CourseId = request.CourseId, EnrollDate = request.EnrollDate, Status = request.Status };
        
        // Tạo mới đăng ký học tập thông qua service
        var created = await _service.CreateAsync(model);
        
        return CreatedAtAction(nameof(GetById), new { id = created.EnrollmentId }, ApiResponse<EnrollmentResponse>.SuccessResponse(MapToResponse(created), "Enrollment created successfully"));
    }

    /// <summary>
    /// Cập nhật thông tin đăng ký học tập.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEnrollmentRequest request)
    {
        // Kiểm tra tính hợp lệ của dữ liệu đầu vào
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", ModelState));
        
        var model = new EnrollmentModel { StudentId = request.StudentId, CourseId = request.CourseId, EnrollDate = request.EnrollDate, Status = request.Status };
        
        // Cập nhật thông tin qua service
        var updated = await _service.UpdateAsync(id, model);
        
        // Trả về lỗi 404 nếu không tìm thấy bản ghi đăng ký cần cập nhật
        if (updated == null) return NotFound(ApiResponse<object>.ErrorResponse($"Enrollment with id {id} not found"));
        
        return Ok(ApiResponse<EnrollmentResponse>.SuccessResponse(MapToResponse(updated), "Enrollment updated successfully"));
    }

    /// <summary>
    /// Hủy/Xóa thông tin đăng ký học tập.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Thực hiện xóa đăng ký học tập
        var deleted = await _service.DeleteAsync(id);
        
        // Trả về lỗi 404 nếu bản ghi đăng ký không tồn tại
        if (!deleted) return NotFound(ApiResponse<object>.ErrorResponse($"Enrollment with id {id} not found"));
        
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Enrollment deleted successfully"));
    }

    // Chuyển đổi từ Business Model sang Response Model
    private static EnrollmentResponse MapToResponse(EnrollmentModel m) => new()
    {
        EnrollmentId = m.EnrollmentId, StudentId = m.StudentId, CourseId = m.CourseId, EnrollDate = m.EnrollDate, Status = m.Status,
        Student = m.Student != null ? new StudentResponse { StudentId = m.Student.StudentId, FullName = m.Student.FullName, Email = m.Student.Email, DateOfBirth = m.Student.DateOfBirth } : null,
        Course = m.Course != null ? new CourseResponse { CourseId = m.Course.CourseId, CourseName = m.Course.CourseName, SemesterId = m.Course.SemesterId, SubjectId = m.Course.SubjectId } : null
    };
}
