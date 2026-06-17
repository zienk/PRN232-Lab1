using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.API.Controllers;

// StudentsController - quản lý sinh viên
// Lab 2: thêm [Authorize], API Versioning, Binding types, Named Routes
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
// Route với version: /api/v1/students hoặc /api/v2/students
[Route("api/v{version:apiVersion}/students")]
[Produces("application/json", "application/xml")] // Content Negotiation: hỗ trợ JSON và XML
[Authorize(Policy = "Lab2Authenticated")] // Version 2 yêu cầu đăng nhập, version 1 mở như Lab 1
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>
    /// Lấy danh sách sinh viên (có phân trang, tìm kiếm, sắp xếp)
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(PagedResponseModel<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(
        // [FromQuery]: lấy tham số từ query string (?search=...)
        [FromQuery] string? search,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null,
        // [FromHeader]: lấy giá trị từ HTTP header
        // Lab 2 yêu cầu có Header Binding với X-Request-Id
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _studentService.GetStudentsAsync(search, sort, page, size, fields, expand);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin một sinh viên theo ID
    /// </summary>
    // [HttpGet("{id:int}")]: Route constraint - chỉ nhận ID là số nguyên
    // Name = "GetStudentById": Named Route - dùng trong CreatedAtRoute()
    [HttpGet("{id:int}", Name = "GetStudentById")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ResponseModel<StudentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        // [FromRoute]: lấy giá trị từ URL path (/students/5)
        [FromRoute] int id,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _studentService.GetStudentByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Tạo sinh viên mới (chỉ Admin)
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    // [Authorize(Policy = "Lab2Admin")]: chỉ Admin mới được tạo sinh viên
    [Authorize(Policy = "Lab2Admin")]
    [ProducesResponseType(typeof(ResponseModel<StudentResponseModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        // [FromBody]: lấy dữ liệu từ request body (JSON/XML)
        [FromBody] StudentRequestModel model,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        // Kiểm tra validation (Data Annotations + FluentValidation)
        this.EchoRequestId(requestId);
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Dữ liệu không hợp lệ",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
            });
        }

        var result = await _studentService.CreateStudentAsync(model);
        // CreatedAtRoute: trả về 201 kèm Location header trỏ đến resource mới
        return CreatedAtRoute(
            "GetStudentById",
            new { version = RouteData.Values["version"]?.ToString() ?? "1.0", id = result.Data?.StudentId },
            result);
    }

    /// <summary>
    /// Cập nhật thông tin sinh viên (chỉ Admin)
    /// </summary>
    // Route constraint {id:int}: đảm bảo id là số nguyên
    [HttpPut("{id:int}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Policy = "Lab2Admin")]
    [ProducesResponseType(typeof(ResponseModel<StudentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] StudentRequestModel model,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Dữ liệu không hợp lệ",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
            });
        }

        var result = await _studentService.UpdateStudentAsync(id, model);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Xóa sinh viên (chỉ Admin - Admin-only endpoint theo yêu cầu Lab 2)
    /// </summary>
    [HttpDelete("{id:int}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    // Đây là admin-only endpoint được yêu cầu bởi Lab 2
    [Authorize(Policy = "Lab2Admin")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _studentService.DeleteStudentAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
}
