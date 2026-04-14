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
    public class OfficeController : ControllerBase
    {
        private readonly IOfficeService _officeService;
        private readonly ILogger<OfficeController> _logger;

        public OfficeController(IOfficeService officeService, ILogger<OfficeController> logger)
        {
            _officeService = officeService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateOffice([FromBody] CreateOfficeDto request)
        {
            try
            {
                var result = await _officeService.CreateOfficeAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating office: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOfficeById(int id)
        {
            var office = await _officeService.GetOfficeByIdAsync(id);
            if (office == null)
            {
                return NotFound(new { message = "Office not found" });
            }
            return Ok(office);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOffices()
        {
            var offices = await _officeService.GetAllOfficesAsync();
            return Ok(offices);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetOfficesByStatus(string status)
        {
            if (!Enum.TryParse<AvailabilityStatus>(status, true, out var availabilityStatus))
            {
                return BadRequest(new { message = "Invalid status. Valid values: OPEN, CLOSED, BUSY, AWAY" });
            }

            var offices = await _officeService.GetOfficesByStatusAsync(availabilityStatus);
            return Ok(offices);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOfficeByUserId(int userId)
        {
            var office = await _officeService.GetOfficeByUserIdAsync(userId);
            if (office == null)
            {
                return NotFound(new { message = "Office not found for this user" });
            }
            return Ok(office);
        }

        [HttpGet("my-office")]
        public async Task<IActionResult> GetMyOffice()
        {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var office = await _officeService.GetOfficeByUserIdAsync(userId);
            if (office == null)
            {
                return NotFound(new { message = "You don't have an office assigned" });
            }
            return Ok(office);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateOffice(int id, [FromBody] CreateOfficeDto request)
        {
            try
            {
                var result = await _officeService.UpdateOfficeAsync(id, request);
                if (result == null)
                {
                    return NotFound(new { message = "Office not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating office: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/availability")]
        public async Task<IActionResult> UpdateOfficeAvailability(int id, [FromBody] UpdateOfficeAvailabilityDto request)
        {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value 
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst("Role")?.Value;

            // Get the office to check ownership
            var office = await _officeService.GetOfficeByIdAsync(id);
            if (office == null)
            {
                return NotFound(new { message = "Office not found" });
            }

            // Users can only update their own office availability unless they are admin
            if (userRole != "ADMIN" && userIdClaim != office.StaffUserId.ToString())
            {
                return Forbid();
            }

            var result = await _officeService.UpdateOfficeAvailabilityAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteOffice(int id)
        {
            try
            {
                var result = await _officeService.DeleteOfficeAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Office not found" });
                }
                return Ok(new { message = "Office deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting office: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
