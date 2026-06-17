using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.API.Extensions;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;

namespace PRN232.LMS.API.Controllers;

// SemestersController - quản lý học kỳ
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/semesters")]
[Produces("application/json", "application/xml")]
[Authorize(Policy = "Lab2Authenticated")]
public class SemestersController : ControllerBase
{
    private readonly ISemesterService _semesterService;

    public SemestersController(ISemesterService semesterService)
    {
        _semesterService = semesterService;
    }

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
        var result = await _semesterService.GetSemestersAsync(search, sort, page, size, fields, expand);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = "GetSemesterById")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [ProducesResponseType(typeof(ResponseModel<SemesterResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _semesterService.GetSemesterByIdAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Policy = "Lab2Admin")]
    [ProducesResponseType(typeof(ResponseModel<SemesterResponseModel>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] SemesterRequestModel model,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        if (!ModelState.IsValid)
            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

        var result = await _semesterService.CreateSemesterAsync(model);
        return CreatedAtRoute(
            "GetSemesterById",
            new { version = RouteData.Values["version"]?.ToString() ?? "1.0", id = result.Data?.SemesterId },
            result);
    }

    [HttpPut("{id:int}")]
    [MapToApiVersion("1.0")]
    [MapToApiVersion("2.0")]
    [Authorize(Policy = "Lab2Admin")]
    [ProducesResponseType(typeof(ResponseModel<SemesterResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] SemesterRequestModel model,
        [FromHeader(Name = "X-Request-Id")] string? requestId = null)
    {
        this.EchoRequestId(requestId);
        var result = await _semesterService.UpdateSemesterAsync(id, model);
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
        var result = await _semesterService.DeleteSemesterAsync(id);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
}
