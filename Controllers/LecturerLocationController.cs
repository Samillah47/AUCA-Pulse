using AUCAPulse.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AUCAPulse.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Route("api/lecturer-locations")]
    [Authorize]
    public class LecturerLocationController : ControllerBase
    {
        private readonly ILecturerLocationService _service;

        public LecturerLocationController(ILecturerLocationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetForAllLecturersAsync();
            return Ok(list);
        }

        [HttpGet("{lecturerId}")]
        public async Task<IActionResult> GetForLecturer(int lecturerId)
        {
            var location = await _service.GetForLecturerAsync(lecturerId);
            if (location == null)
                return NotFound(new { message = "Lecturer not found" });
            return Ok(location);
        }
    }
}
