using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/courses")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
        {
            try
            {
                var course = await _courseService.CreateCourseAsync(dto);
                return Ok(new { message = "Course created successfully", data = course });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound(new { message = "Course not found" });
            return Ok(course);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        [HttpGet("department/{department}")]
        public async Task<IActionResult> GetCoursesByDepartment(string department)
        {
            var courses = await _courseService.GetCoursesByDepartmentAsync(department);
            return Ok(courses);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {
            try
            {
                var course = await _courseService.UpdateCourseAsync(id, dto);
                if (course == null)
                    return NotFound(new { message = "Course not found" });
                return Ok(new { message = "Course updated successfully", data = course });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var result = await _courseService.DeleteCourseAsync(id);
                if (!result)
                    return NotFound(new { message = "Course not found" });
                return Ok(new { message = "Course deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
