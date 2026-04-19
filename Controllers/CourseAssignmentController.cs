using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/courseassignments")]
    [Authorize]
    public class CourseAssignmentController : ControllerBase
    {
        private readonly ICourseAssignmentService _service;

        public CourseAssignmentController(ICourseAssignmentService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateCourseAssignmentDto dto)
        {
            try
            {
                var assignment = await _service.CreateAssignmentAsync(dto);
                return Ok(new { message = "Course assigned successfully", data = assignment });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateBulk([FromBody] BulkCreateCourseAssignmentDto dto)
        {
            try
            {
                var result = await _service.CreateBulkAssignmentsAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var assignment = await _service.GetAssignmentByIdAsync(id);
            if (assignment == null)
                return NotFound(new { message = "Assignment not found" });
            return Ok(assignment);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assignments = await _service.GetAllAssignmentsAsync();
            return Ok(assignments);
        }

        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetByLecturer(int lecturerId)
        {
            var assignments = await _service.GetAssignmentsByLecturerAsync(lecturerId);
            return Ok(assignments);
        }

        [HttpGet("semester/{semesterId}")]
        public async Task<IActionResult> GetBySemester(int semesterId)
        {
            var assignments = await _service.GetAssignmentsBySemesterAsync(semesterId);
            return Ok(assignments);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            var assignments = await _service.GetAssignmentsByCourseAsync(courseId);
            return Ok(assignments);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAssignmentAsync(id);
            if (!result)
                return NotFound(new { message = "Assignment not found" });
            return Ok(new { message = "Assignment removed successfully" });
        }
    }
}
