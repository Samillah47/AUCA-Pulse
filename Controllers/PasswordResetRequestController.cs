using AUCAPulse.DTOs.Request;
using AUCAPulse.Models;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/passwordresetrequests")]
    public class PasswordResetRequestController : ControllerBase
    {
        private readonly IPasswordResetRequestService _service;

        public PasswordResetRequestController(IPasswordResetRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] CreatePasswordResetRequestDto dto)
        {
            try
            {
                var request = await _service.CreateRequestAsync(dto);
                return Ok(new { message = "Password reset request created successfully", data = request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var request = await _service.GetRequestByIdAsync(id);
            if (request == null)
                return NotFound(new { message = "Password reset request not found" });

            return Ok(new { data = request });
        }

        [HttpGet("token/{token}")]
        public async Task<IActionResult> GetRequestByToken(string token)
        {
            var request = await _service.GetRequestByTokenAsync(token);
            if (request == null)
                return NotFound(new { message = "Password reset request not found" });

            return Ok(new { data = request });
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _service.GetAllRequestsAsync();
            return Ok(new { data = requests });
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetRequestsByStatus(RequestStatus status)
        {
            var requests = await _service.GetRequestsByStatusAsync(status);
            return Ok(new { data = requests });
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetRequestsByUserId(int userId)
        {
            var requests = await _service.GetRequestsByUserIdAsync(userId);
            return Ok(new { data = requests });
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdatePasswordResetRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "User not authenticated" });

            var reviewedBy = int.Parse(userIdClaim);
            var request = await _service.UpdateRequestStatusAsync(id, dto, reviewedBy);

            if (request == null)
                return NotFound(new { message = "Password reset request not found" });

            return Ok(new { message = "Password reset request status updated successfully", data = request });
        }

        [HttpGet("validate/{token}")]
        public async Task<IActionResult> ValidateToken(string token)
        {
            var isValid = await _service.ValidateTokenAsync(token);
            return Ok(new { valid = isValid });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var result = await _service.DeleteRequestAsync(id);
            if (!result)
                return NotFound(new { message = "Password reset request not found" });

            return Ok(new { message = "Password reset request deleted successfully" });
        }
    }
}
