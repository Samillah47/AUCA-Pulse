using AUCAPulse.DTOs.Request;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/appointments")]
    [Authorize]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(IAppointmentService appointmentService, ILogger<AppointmentController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        // POST /api/Appointment — students request an appointment with a staff / lecturer
        [HttpPost]
        [Authorize(Roles = "STUDENT")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto request)
        {
            try
            {
                var studentIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(studentIdClaim) || !int.TryParse(studentIdClaim, out var studentUserId))
                {
                    return Unauthorized(new { message = "We couldn't verify your account. Please sign in again." });
                }

                var created = await _appointmentService.CreateAppointmentAsync(studentUserId, request);
                return Ok(new { message = "Appointment request submitted. You'll be notified once it's reviewed.", data = created });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create appointment");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            var list = await _appointmentService.GetAppointmentsByStudentAsync(studentId);
            return Ok(list);
        }

        [HttpGet("staff/{staffId}")]
        public async Task<IActionResult> GetByStaff(int staffId)
        {
            var list = await _appointmentService.GetAppointmentsByStaffAsync(staffId);
            return Ok(list);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "STAFF,LECTURER,ADMIN")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto request)
        {
            try
            {
                var actingIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? User.FindFirst("UserId")?.Value;
                int? actingStaffId = int.TryParse(actingIdClaim, out var v) ? v : (int?)null;

                var updated = await _appointmentService.UpdateAppointmentStatusAsync(id, request, actingStaffId);
                if (updated == null)
                    return NotFound(new { message = "Appointment not found." });

                return Ok(new { message = "Appointment updated.", data = updated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update appointment {AppointmentId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var ok = await _appointmentService.DeleteAppointmentAsync(id);
                if (!ok) return NotFound(new { message = "Appointment not found." });
                return Ok(new { message = "Appointment cancelled." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
