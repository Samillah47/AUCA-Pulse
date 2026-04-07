using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LectureScheduleController : ControllerBase
    {
        private readonly ILectureScheduleService _lectureScheduleService;
        private readonly ILogger<LectureScheduleController> _logger;

        public LectureScheduleController(ILectureScheduleService lectureScheduleService, ILogger<LectureScheduleController> logger)
        {
            _lectureScheduleService = lectureScheduleService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "LECTURER,ADMIN")]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateLectureScheduleDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int lecturerId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _lectureScheduleService.CreateScheduleAsync(lecturerId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating lecture schedule: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleById(int id)
        {
            var schedule = await _lectureScheduleService.GetScheduleByIdAsync(id);
            if (schedule == null)
            {
                return NotFound(new { message = "Lecture schedule not found" });
            }
            return Ok(schedule);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSchedules()
        {
            var schedules = await _lectureScheduleService.GetAllSchedulesAsync();
            return Ok(schedules);
        }

        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetSchedulesByLecturerId(int lecturerId)
        {
            var schedules = await _lectureScheduleService.GetSchedulesByLecturerIdAsync(lecturerId);
            return Ok(schedules);
        }

        [HttpGet("my-schedule")]
        [Authorize(Roles = "LECTURER,ADMIN")]
        public async Task<IActionResult> GetMySchedule()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int lecturerId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var schedules = await _lectureScheduleService.GetSchedulesByLecturerIdAsync(lecturerId);
            return Ok(schedules);
        }

        [HttpGet("semester/{semesterId}")]
        public async Task<IActionResult> GetSchedulesBySemesterId(int semesterId)
        {
            var schedules = await _lectureScheduleService.GetSchedulesBySemesterIdAsync(semesterId);
            return Ok(schedules);
        }

        [HttpGet("day/{day}")]
        public async Task<IActionResult> GetSchedulesByDay(string day)
        {
            var schedules = await _lectureScheduleService.GetSchedulesByDayAsync(day.ToUpper());
            return Ok(schedules);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "LECTURER,ADMIN")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] CreateLectureScheduleDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst("Role")?.Value;

                // Get the schedule to check ownership
                var existingSchedule = await _lectureScheduleService.GetScheduleByIdAsync(id);
                if (existingSchedule == null)
                {
                    return NotFound(new { message = "Lecture schedule not found" });
                }

                // Lecturers can only update their own schedules unless they are admin
                if (userRole != "ADMIN" && userIdClaim != existingSchedule.LecturerId.ToString())
                {
                    return Forbid();
                }

                var result = await _lectureScheduleService.UpdateScheduleAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating lecture schedule: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "LECTURER,ADMIN")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst("Role")?.Value;

                // Get the schedule to check ownership
                var existingSchedule = await _lectureScheduleService.GetScheduleByIdAsync(id);
                if (existingSchedule == null)
                {
                    return NotFound(new { message = "Lecture schedule not found" });
                }

                // Lecturers can only delete their own schedules unless they are admin
                if (userRole != "ADMIN" && userIdClaim != existingSchedule.LecturerId.ToString())
                {
                    return Forbid();
                }

                var result = await _lectureScheduleService.DeleteScheduleAsync(id);
                return Ok(new { message = "Lecture schedule deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting lecture schedule: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
