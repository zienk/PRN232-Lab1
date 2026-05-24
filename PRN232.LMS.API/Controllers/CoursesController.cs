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
public class CoursesController : ControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all courses with search, sort, paging, field selection, and expansion support.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<CourseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters parameters)
    {
        var result = await _service.GetAllAsync(parameters);
        var responses = result.Items.Select(MapToResponse).ToList();
        var data = FieldSelector.SelectFieldsList(responses, parameters.Fields);

        return Ok(new PaginatedResponse<CourseResponse>
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
    /// Get a course by ID with related semester and subject data.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var model = await _service.GetByIdAsync(id, expand);
        if (model == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Course with id {id} not found"));
        return Ok(ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(model)));
    }

    /// <summary>
    /// Create a new course.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", ModelState));

        var model = new CourseModel
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId
        };

        var created = await _service.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.CourseId },
            ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(created), "Course created successfully"));
    }

    /// <summary>
    /// Update an existing course.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", ModelState));

        var model = new CourseModel
        {
            CourseName = request.CourseName,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId
        };

        var updated = await _service.UpdateAsync(id, model);
        if (updated == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Course with id {id} not found"));

        return Ok(ApiResponse<CourseResponse>.SuccessResponse(MapToResponse(updated), "Course updated successfully"));
    }

    /// <summary>
    /// Delete a course by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.ErrorResponse($"Course with id {id} not found"));
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Course deleted successfully"));
    }

    private static CourseResponse MapToResponse(CourseModel model) => new()
    {
        CourseId = model.CourseId,
        CourseName = model.CourseName,
        SemesterId = model.SemesterId,
        SubjectId = model.SubjectId,
        Semester = model.Semester != null ? new SemesterResponse
        {
            SemesterId = model.Semester.SemesterId,
            SemesterName = model.Semester.SemesterName,
            StartDate = model.Semester.StartDate,
            EndDate = model.Semester.EndDate
        } : null,
        Subject = model.Subject != null ? new SubjectResponse
        {
            SubjectId = model.Subject.SubjectId,
            SubjectCode = model.Subject.SubjectCode,
            SubjectName = model.Subject.SubjectName,
            Credit = model.Subject.Credit
        } : null
    };
}
