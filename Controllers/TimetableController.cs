using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/timetable")]
    [Authorize]
    public class TimetableController : ControllerBase
    {
        private readonly ITimetableGeneratorService _generator;

        public TimetableController(ITimetableGeneratorService generator)
        {
            _generator = generator;
        }

        [HttpPost("generate")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Generate([FromBody] GenerateTimetableDto dto)
        {
            try
            {
                var result = await _generator.GenerateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
