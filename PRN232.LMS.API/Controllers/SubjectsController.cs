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
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _service;

    public SubjectsController(ISubjectService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all subjects with search, sort, paging, field selection, and expansion support.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters parameters)
    {
        var result = await _service.GetAllAsync(parameters);
        var responses = result.Items.Select(MapToResponse).ToList();
        var data = FieldSelector.SelectFieldsList(responses, parameters.Fields);

        return Ok(new PaginatedResponse<SubjectResponse>
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
    /// Get a subject by ID with optional expansion.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, [FromQuery] string? expand = null)
    {
        var model = await _service.GetByIdAsync(id, expand);
        if (model == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Subject with id {id} not found"));
        return Ok(ApiResponse<SubjectResponse>.SuccessResponse(MapToResponse(model)));
    }

    /// <summary>
    /// Create a new subject.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", ModelState));

        var model = new SubjectModel
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };

        var created = await _service.CreateAsync(model);
        return CreatedAtAction(nameof(GetById), new { id = created.SubjectId },
            ApiResponse<SubjectResponse>.SuccessResponse(MapToResponse(created), "Subject created successfully"));
    }

    /// <summary>
    /// Update an existing subject.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", ModelState));

        var model = new SubjectModel
        {
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            Credit = request.Credit
        };

        var updated = await _service.UpdateAsync(id, model);
        if (updated == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Subject with id {id} not found"));

        return Ok(ApiResponse<SubjectResponse>.SuccessResponse(MapToResponse(updated), "Subject updated successfully"));
    }

    /// <summary>
    /// Delete a subject by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(ApiResponse<object>.ErrorResponse($"Subject with id {id} not found"));
        return Ok(ApiResponse<object>.SuccessResponse(new { id }, "Subject deleted successfully"));
    }

    private static SubjectResponse MapToResponse(SubjectModel model) => new()
    {
        SubjectId = model.SubjectId,
        SubjectCode = model.SubjectCode,
        SubjectName = model.SubjectName,
        Credit = model.Credit,
        Courses = model.Courses?.Select(c => new CourseResponse
        {
            CourseId = c.CourseId,
            CourseName = c.CourseName,
            SemesterId = c.SemesterId,
            SubjectId = c.SubjectId
        }).ToList()
    };
}
