using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SemesterController : ControllerBase
    {
        private readonly ISemesterService _semesterService;

        public SemesterController(ISemesterService semesterService)
        {
            _semesterService = semesterService;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateSemester([FromBody] CreateSemesterDto dto)
        {
            try
            {
                var semester = await _semesterService.CreateSemesterAsync(dto);
                return Ok(new { message = "Semester created successfully", data = semester });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSemesterById(int id)
        {
            var semester = await _semesterService.GetSemesterByIdAsync(id);
            if (semester == null)
                return NotFound(new { message = "Semester not found" });

            return Ok(new { data = semester });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSemesters()
        {
            var semesters = await _semesterService.GetAllSemestersAsync();
            return Ok(new { data = semesters });
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSemester()
        {
            var semester = await _semesterService.GetCurrentSemesterAsync();
            if (semester == null)
                return NotFound(new { message = "No current semester found" });

            return Ok(new { data = semester });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateSemester(int id, [FromBody] UpdateSemesterDto dto)
        {
            try
            {
                var semester = await _semesterService.UpdateSemesterAsync(id, dto);
                if (semester == null)
                    return NotFound(new { message = "Semester not found" });

                return Ok(new { message = "Semester updated successfully", data = semester });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/set-current")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SetCurrentSemester(int id)
        {
            var result = await _semesterService.SetCurrentSemesterAsync(id);
            if (!result)
                return NotFound(new { message = "Semester not found" });

            return Ok(new { message = "Current semester set successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteSemester(int id)
        {
            var result = await _semesterService.DeleteSemesterAsync(id);
            if (!result)
                return NotFound(new { message = "Semester not found" });

            return Ok(new { message = "Semester deleted successfully" });
        }
    }
}
