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
public class StudentsController : ControllerBase
{
    private readonly IStudentService _service;
    
    // Tiêm dependency StudentService vào controller
    public StudentsController(IStudentService service) => _service = service;

    /// <summary>
    /// Lấy danh sách học sinh có hỗ trợ tìm kiếm, sắp xếp, phân trang, lọc trường và mở rộng dữ liệu.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<StudentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters parameters)
    {
        // Gọi service lấy danh sách học sinh đã phân trang
        var result = await _service.GetAllAsync(parameters);
        
        // Map danh sách Business Model sang Response Model
        var responses = result.Items.Select(MapToResponse).ToList();
        
        // Thực hiện lọc trường động nếu có yêu cầu fields
        var data = FieldSelector.SelectFieldsList(responses, parameters.Fields);
        
        return Ok(new PaginatedResponse<StudentResponse>
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
    /// Lấy chi tiết thông tin học sinh theo ID kèm danh sách đăng ký học tập.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        // Tìm kiếm thông tin học sinh theo ID
        var model = await _service.GetByIdAsync(id, expand);
        
        // Trả về lỗi 404 nếu không tìm thấy học sinh
        if (model == null) return NotFound(ApiResponse<object>.ErrorResponse($"Student with id {id} not found"));
        
        return Ok(ApiResponse<StudentResponse>.SuccessResponse(MapToResponse(model)));
    }

    /// <summary>
    /// Tạo mới một học sinh.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
    {
        // Kiểm tra tính hợp lệ của dữ liệu đầu vào
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", ModelState));
        
        var model = new StudentModel { FullName = request.FullName, Email = request.Email, DateOfBirth = request.DateOfBirth };
        
        // Gọi service tạo mới học sinh
        var created = await _service.CreateAsync(model);
        
        return CreatedAtAction(nameof(GetById), new { id = created.StudentId }, ApiResponse<StudentResponse>.SuccessResponse(MapToResponse(created), "Student created successfully"));
    }

    /// <summary>
    /// Cập nhật thông tin học sinh.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
    {
        // Kiểm tra tính hợp lệ của dữ liệu đầu vào
        if (!ModelState.IsValid) return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", ModelState));
        
        var model = new StudentModel { FullName = request.FullName, Email = request.Email, DateOfBirth = request.DateOfBirth };
        
        // Thực hiện cập nhật thông tin học sinh qua service
        var updated = await _service.UpdateAsync(id, model);
        
        // Trả về lỗi 404 nếu không tìm thấy học sinh cần cập nhật
        if (updated == null) return NotFound(ApiResponse<object>.ErrorResponse($"Student with id {id} not found"));
        
        return Ok(ApiResponse<StudentResponse>.SuccessResponse(MapToResponse(updated), "Student updated successfully"));
    }

    /// <summary>
    /// Xóa học sinh theo ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Thực hiện xóa học sinh
        var deleted = await _service.DeleteAsync(id);
        
        // Trả về 404 nếu học sinh cần xóa không tồn tại
        if (!deleted) return NotFound(ApiResponse<object>.ErrorResponse($"Student with id {id} not found"));
        
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Student deleted successfully"));
    }

    // Chuyển đổi từ Business Model sang Response Model
    private static StudentResponse MapToResponse(StudentModel m) => new()
    {
        StudentId = m.StudentId, FullName = m.FullName, Email = m.Email, DateOfBirth = m.DateOfBirth,
        Enrollments = m.Enrollments?.Select(e => new EnrollmentResponse { EnrollmentId = e.EnrollmentId, StudentId = e.StudentId, CourseId = e.CourseId, EnrollDate = e.EnrollDate, Status = e.Status }).ToList()
    };
}
