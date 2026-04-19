using AUCAPulse.DTOs.Request;
using AUCAPulse.Models;
using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/rooms")]
    [Authorize]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly IRoundRobinRoomService _roundRobinRoomService;
        private readonly ILogger<RoomController> _logger;

        public RoomController(
            IRoomService roomService,
            IRoundRobinRoomService roundRobinRoomService,
            ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _roundRobinRoomService = roundRobinRoomService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto request)
        {
            try
            {
                var result = await _roomService.CreateRoomAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating room: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound(new { message = "Room not found" });
            }
            return Ok(room);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetRoomsByStatus(string status)
        {
            if (!Enum.TryParse<RoomStatus>(status, true, out var roomStatus))
            {
                return BadRequest(new { message = "Invalid status. Valid values: AVAILABLE, OCCUPIED, MAINTENANCE" });
            }

            var rooms = await _roomService.GetRoomsByStatusAsync(roomStatus);
            return Ok(rooms);
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetRoomsByType(string type)
        {
            if (!Enum.TryParse<RoomType>(type, true, out var roomType))
            {
                return BadRequest(new { message = "Invalid type. Valid values: CLASSROOM, LABORATORY, OFFICE, CONFERENCE, LIBRARY" });
            }

            var rooms = await _roomService.GetRoomsByTypeAsync(roomType);
            return Ok(rooms);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms()
        {
            var rooms = await _roomService.GetAvailableRoomsAsync();
            return Ok(rooms);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] CreateRoomDto request)
        {
            try
            {
                var result = await _roomService.UpdateRoomAsync(id, request);
                if (result == null)
                {
                    return NotFound(new { message = "Room not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating room: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("auto-assign")]
        [Authorize(Roles = "LECTURER,STAFF,ADMIN")]
        public async Task<IActionResult> AutoAssignRoom([FromBody] AutoAssignRoomDto request)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                  ?? User.FindFirst("UserId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                RoomType? typeFilter = null;
                if (!string.IsNullOrWhiteSpace(request.RoomType))
                {
                    if (!Enum.TryParse<RoomType>(request.RoomType, true, out var parsed))
                    {
                        return BadRequest(new { message = "Invalid room type" });
                    }
                    typeFilter = parsed;
                }

                var result = await _roundRobinRoomService.AssignNextAvailableRoomAsync(
                    userId, typeFilter, request.DurationMinutes);

                if (result == null)
                {
                    return NotFound(new { message = "No available rooms match your criteria" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error auto-assigning room: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/occupy")]
        [Authorize(Roles = "LECTURER")]
        public async Task<IActionResult> OccupyRoom(int id, [FromBody] OccupyRoomDto request)
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

                var result = await _roomService.OccupyRoomAsync(id, lecturerId, request);
                if (result == null)
                {
                    return NotFound(new { message = "Room not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occupying room: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/release")]
        [Authorize(Roles = "LECTURER,STAFF,ADMIN")]
        public async Task<IActionResult> ReleaseRoom(int id)
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

                var result = await _roomService.ReleaseRoomAsync(id, lecturerId);
                if (result == null)
                {
                    return NotFound(new { message = "Room not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error releasing room: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var result = await _roomService.DeleteRoomAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Room not found" });
                }
                return Ok(new { message = "Room deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting room: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
