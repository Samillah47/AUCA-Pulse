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
    public class VerificationRequestController : ControllerBase
    {
        private readonly IVerificationRequestService _verificationRequestService;
        private readonly ILogger<VerificationRequestController> _logger;

        public VerificationRequestController(IVerificationRequestService verificationRequestService, ILogger<VerificationRequestController> logger)
        {
            _verificationRequestService = verificationRequestService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] CreateVerificationRequestDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _verificationRequestService.CreateRequestAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating verification request: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var request = await _verificationRequestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new { message = "Verification request not found" });
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst("Role")?.Value;

            // Users can only view their own requests unless they are admin
            if (userRole != "ADMIN" && userIdClaim != request.UserId.ToString())
            {
                return Forbid();
            }

            return Ok(request);
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllRequests()
        {
            var requests = await _verificationRequestService.GetAllRequestsAsync();
            return Ok(requests);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetRequestsByUserId(int userId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            var userRole = User.FindFirst("Role")?.Value;

            // Users can only view their own requests unless they are admin
            if (userRole != "ADMIN" && userIdClaim != userId.ToString())
            {
                return Forbid();
            }

            var requests = await _verificationRequestService.GetRequestsByUserIdAsync(userId);
            return Ok(requests);
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            var requests = await _verificationRequestService.GetRequestsByUserIdAsync(userId);
            return Ok(requests);
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetRequestsByStatus(string status)
        {
            if (!Enum.TryParse<VerificationStatus>(status, true, out var verificationStatus))
            {
                return BadRequest(new { message = "Invalid status. Valid values: PENDING, APPROVED, REJECTED" });
            }

            var requests = await _verificationRequestService.GetRequestsByStatusAsync(verificationStatus);
            return Ok(requests);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateRequestStatus(int id, [FromBody] UpdateVerificationRequestDto request)
        {
            var adminIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
            {
                return Unauthorized(new { message = "Invalid admin token" });
            }

            var result = await _verificationRequestService.UpdateRequestStatusAsync(id, adminId, request);
            if (result == null)
            {
                return NotFound(new { message = "Verification request not found" });
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var result = await _verificationRequestService.DeleteRequestAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Verification request not found" });
            }

            return Ok(new { message = "Verification request deleted successfully" });
        }
    }
}
