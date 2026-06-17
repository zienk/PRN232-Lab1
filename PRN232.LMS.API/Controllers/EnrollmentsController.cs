using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.API.Controllers;

// EnrollmentsController - quản lý đăng ký học
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/enrollments")]
[Produces("application/json", "application/xml")]
[Authorize(Policy = "Lab2Authenticated")]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(PagedResponseModel<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] int? studentId,
        [FromQuery] int? courseId,
        [FromQuery] string? status,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        [FromQuery] string? fields = null,
        [FromQuery] string? expand = null,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _enrollmentService.GetEnrollmentsAsync(studentId, courseId, status, sort, page, size, fields, expand);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = "GetEnrollmentById")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ResponseModel<EnrollmentResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _enrollmentService.GetEnrollmentByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Policy = "Lab2Admin")]
    [ProducesResponseType(typeof(ResponseModel<EnrollmentResponseModel>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] EnrollmentRequestModel model,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

        var result = await _enrollmentService.CreateEnrollmentAsync(model);
        return CreatedAtRoute(
            "GetEnrollmentById",
            new { version = RouteData.Values["version"]?.ToString() ?? "1.0", id = result.Data?.EnrollmentId },
            result);
    }

    [HttpPut("{id:int}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Policy = "Lab2Admin")]
    [ProducesResponseType(typeof(ResponseModel<EnrollmentResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] EnrollmentRequestModel model,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _enrollmentService.UpdateEnrollmentAsync(id, model);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Policy = "Lab2Admin")]
    [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(
        [FromRoute] int id,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _enrollmentService.DeleteEnrollmentAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
}
