using AUCAPulse.DTOs.Request;
using AUCAPulse.Models;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LecturerStatusController : ControllerBase
    {
        private readonly ILecturerStatusService _lecturerStatusService;
        private readonly ILogger<LecturerStatusController> _logger;

        public LecturerStatusController(ILecturerStatusService lecturerStatusService, ILogger<LecturerStatusController> logger)
        {
            _lecturerStatusService = lecturerStatusService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "LECTURER,STAFF,ADMIN")]
        public async Task<IActionResult> CreateStatus([FromBody] CreateLecturerStatusDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int lecturerId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _lecturerStatusService.CreateStatusAsync(lecturerId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating lecturer status: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStatusById(int id)
        {
            var status = await _lecturerStatusService.GetStatusByIdAsync(id);
            if (status == null)
            {
                return NotFound(new { message = "Lecturer status not found" });
            }
            return Ok(status);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStatuses()
        {
            var statuses = await _lecturerStatusService.GetAllStatusesAsync();
            return Ok(statuses);
        }

        [HttpGet("lecturer/{lecturerId}")]
        public async Task<IActionResult> GetStatusesByLecturerId(int lecturerId)
        {
            var statuses = await _lecturerStatusService.GetStatusesByLecturerIdAsync(lecturerId);
            return Ok(statuses);
        }

        [HttpGet("lecturer/{lecturerId}/current")]
        public async Task<IActionResult> GetCurrentStatusByLecturerId(int lecturerId)
        {
            var status = await _lecturerStatusService.GetCurrentStatusByLecturerIdAsync(lecturerId);
            if (status == null)
            {
                return NotFound(new { message = "No status found for this lecturer" });
            }
            return Ok(status);
        }

        [HttpGet("my-status")]
        [Authorize(Roles = "LECTURER,STAFF,ADMIN")]
        public async Task<IActionResult> GetMyCurrentStatus()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int lecturerId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var status = await _lecturerStatusService.GetCurrentStatusByLecturerIdAsync(lecturerId);
            if (status == null)
            {
                return NotFound(new { message = "No status found" });
            }
            return Ok(status);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetStatusesByStatusType(string status)
        {
            if (!Enum.TryParse<Status>(status, true, out var statusType))
            {
                return BadRequest(new { message = "Invalid status. Valid values: AVAILABLE, IN_CLASS, IN_MEETING, ON_LEAVE, UNAVAILABLE" });
            }

            var statuses = await _lecturerStatusService.GetStatusesByStatusTypeAsync(statusType);
            return Ok(statuses);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "LECTURER,STAFF,ADMIN")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] CreateLecturerStatusDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst("Role")?.Value;

                // Get the status to check ownership
                var existingStatus = await _lecturerStatusService.GetStatusByIdAsync(id);
                if (existingStatus == null)
                {
                    return NotFound(new { message = "Lecturer status not found" });
                }

                // Lecturers can only update their own status unless they are admin
                if (userRole != "ADMIN" && userIdClaim != existingStatus.LecturerId.ToString())
                {
                    return Forbid();
                }

                var result = await _lecturerStatusService.UpdateStatusAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating lecturer status: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "LECTURER,STAFF,ADMIN")]
        public async Task<IActionResult> DeleteStatus(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("UserId")?.Value;
                var userRole = User.FindFirst("Role")?.Value;

                // Get the status to check ownership
                var existingStatus = await _lecturerStatusService.GetStatusByIdAsync(id);
                if (existingStatus == null)
                {
                    return NotFound(new { message = "Lecturer status not found" });
                }

                // Lecturers can only delete their own status unless they are admin
                if (userRole != "ADMIN" && userIdClaim != existingStatus.LecturerId.ToString())
                {
                    return Forbid();
                }

                var result = await _lecturerStatusService.DeleteStatusAsync(id);
                return Ok(new { message = "Lecturer status deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting lecturer status: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
