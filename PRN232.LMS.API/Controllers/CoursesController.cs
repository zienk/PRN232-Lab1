using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.API.Controllers;

// CoursesController - quản lý khóa học
// Lab 2: thêm versioning, authorization, nested resource endpoint
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/courses")]
[Produces("application/json", "application/xml")]
[Authorize] // Yêu cầu đăng nhập
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>Lấy danh sách khóa học</summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(PagedResponseModel<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _courseService.GetCoursesAsync(search, sort, page, size, fields, expand);
        return Ok(result);
    }

    /// <summary>Lấy khóa học theo ID</summary>
    [HttpGet("{id:int}", Name = "GetCourseById")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ResponseModel<CourseResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _courseService.GetCourseByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách sinh viên của một khóa học (Nested Resource)
    /// Đây là nested resource endpoint: /api/v1/courses/{courseId}/students
    /// Lab 2 yêu cầu có ít nhất 1 nested resource endpoint
    /// </summary>
    [HttpGet("{courseId:int}/students")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(PagedResponseModel<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentsByCourse(
        [FromRoute] int courseId,
        [FromServices] IEnrollmentService enrollmentService,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? expand = null)
    {
        // Lấy danh sách enrollments của course, từ đó ra được danh sách sinh viên
        var result = await enrollmentService.GetEnrollmentsAsync(
            null, courseId, null, null, page, size, null, expand);
        return Ok(result);
    }

    /// <summary>Lấy enrollments của khóa học</summary>
    [HttpGet("{id:int}/enrollments")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetEnrollmentsByCourse(
        [FromServices] IEnrollmentService enrollmentService,
        [FromRoute] int id,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? expand = null)
    {
        var result = await enrollmentService.GetEnrollmentsAsync(null, id, null, null, page, size, null, expand);
        return Ok(result);
    }

    /// <summary>Tạo khóa học mới (chỉ Admin)</summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseModel<CourseResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CourseRequestModel model,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

        var result = await _courseService.CreateCourseAsync(model);
        return CreatedAtRoute(
            "GetCourseById",
            new { version = RouteData.Values["version"]?.ToString() ?? "1.0", id = result.Data?.CourseId },
            result);
    }

    /// <summary>Cập nhật khóa học (chỉ Admin)</summary>
    [HttpPut("{id:int}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseModel<CourseResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] CourseRequestModel model,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _courseService.UpdateCourseAsync(id, model);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>Xóa khóa học (chỉ Admin)</summary>
    [HttpDelete("{id:int}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _courseService.DeleteCourseAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
}
