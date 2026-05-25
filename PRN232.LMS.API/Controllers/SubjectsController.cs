using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models.Requests;
using PRN232.LMS.Services.Models.Responses;
using PRN232.LMS.Services.Models.Common;
using System.Threading.Tasks;

namespace PRN232.LMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectsController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseModel<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? fields = null, [FromQuery] string? expand = null)
        {
            var result = await _subjectService.GetSubjectsAsync(search, sort, page, size, fields, expand);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseModel<SubjectResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _subjectService.GetSubjectByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseModel<SubjectResponseModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SubjectRequestModel model)
        {
            var result = await _subjectService.CreateSubjectAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.SubjectId }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseModel<SubjectResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] SubjectRequestModel model)
        {
            var result = await _subjectService.UpdateSubjectAsync(id, model);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _subjectService.DeleteSubjectAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
    }
}
