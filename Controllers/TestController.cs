using AUCAPulse.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(ApplicationDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                message = "AUCA Pulse API is running"
            });
        }

        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var rolesCount = await _context.Roles.CountAsync();
                var usersCount = await _context.Users.CountAsync();
                var locationsCount = await _context.Locations.CountAsync();
                var roomsCount = await _context.Rooms.CountAsync();
                var officesCount = await _context.Offices.CountAsync();

                return Ok(new
                {
                    status = "connected",
                    database = "auca_pulse_db",
                    tables = new
                    {
                        roles = rolesCount,
                        users = usersCount,
                        locations = locationsCount,
                        rooms = roomsCount,
                        offices = officesCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Database check error: {ex.Message}");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("admin")]
        public async Task<IActionResult> CheckAdmin()
        {
            try
            {
                var admin = await _context.Users
                    .Where(u => u.Email == "habiyaadolphe19@gmail.com")
                    .Select(u => new
                    {
                        id = u.Id,
                        name = u.Name,
                        email = u.Email,
                        role = u.Role.RoleName,
                        status = u.Status.ToString(),
                        identificationNumber = u.IdentificationNumber,
                        department = u.Department,
                        createdAt = u.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (admin == null)
                {
                    return NotFound(new { message = "Admin user not found" });
                }

                return Ok(admin);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Admin check error: {ex.Message}");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _context.Roles.ToListAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get roles error: {ex.Message}");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            try
            {
                var locations = await _context.Locations
                    .Select(l => new
                    {
                        id = l.Id,
                        name = l.Name,
                        code = l.Code,
                        type = l.Type.ToString(),
                        parentId = l.ParentId,
                        createdAt = l.CreatedAt
                    })
                    .OrderBy(l => l.type)
                    .ThenBy(l => l.name)
                    .ToListAsync();
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get locations error: {ex.Message}");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms()
        {
            try
            {
                var rooms = await _context.Rooms
                    .Select(r => new
                    {
                        id = r.Id,
                        roomNumber = r.RoomNumber,
                        roomName = r.RoomName,
                        capacity = r.Capacity,
                        building = r.Building,
                        floor = r.Floor,
                        roomType = r.RoomType.ToString(),
                        status = r.Status.ToString(),
                        currentLecturerId = r.CurrentLecturerId,
                        occupiedAt = r.OccupiedAt,
                        occupiedUntil = r.OccupiedUntil,
                        createdAt = r.CreatedAt
                    })
                    .ToListAsync();
                return Ok(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get rooms error: {ex.Message}");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }
    }
}
