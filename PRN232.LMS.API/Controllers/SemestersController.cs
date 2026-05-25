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
    public class SemestersController : ControllerBase
    {
        private readonly ISemesterService _semesterService;

        public SemestersController(ISemesterService semesterService)
        {
            _semesterService = semesterService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResponseModel<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? fields = null, [FromQuery] string? expand = null)
        {
            var result = await _semesterService.GetSemestersAsync(search, sort, page, size, fields, expand);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ResponseModel<SemesterResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _semesterService.GetSemesterByIdAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseModel<SemesterResponseModel>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SemesterRequestModel model)
        {
            var result = await _semesterService.CreateSemesterAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = result.Data?.SemesterId }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseModel<SemesterResponseModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] SemesterRequestModel model)
        {
            var result = await _semesterService.UpdateSemesterAsync(id, model);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseModel<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _semesterService.DeleteSemesterAsync(id);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }
    }
}
