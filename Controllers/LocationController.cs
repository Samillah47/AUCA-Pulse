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
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ILocationService locationService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationDto request)
        {
            try
            {
                var result = await _locationService.CreateLocationAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating location: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocationById(int id)
        {
            var location = await _locationService.GetLocationByIdAsync(id);
            if (location == null)
            {
                return NotFound(new { message = "Location not found" });
            }
            return Ok(location);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            return Ok(locations);
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetLocationsByType(string type)
        {
            if (!Enum.TryParse<LocationType>(type, true, out var locationType))
            {
                return BadRequest(new { message = "Invalid location type. Valid values: PROVINCE, DISTRICT, SECTOR, CELL, VILLAGE" });
            }

            var locations = await _locationService.GetLocationsByTypeAsync(locationType);
            return Ok(locations);
        }

        [HttpGet("parent/{parentId}")]
        public async Task<IActionResult> GetLocationsByParentId(int parentId)
        {
            var locations = await _locationService.GetLocationsByParentIdAsync(parentId);
            return Ok(locations);
        }

        [HttpGet("roots")]
        public async Task<IActionResult> GetRootLocations()
        {
            var locations = await _locationService.GetRootLocationsAsync();
            return Ok(locations);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] CreateLocationDto request)
        {
            try
            {
                var result = await _locationService.UpdateLocationAsync(id, request);
                if (result == null)
                {
                    return NotFound(new { message = "Location not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating location: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var result = await _locationService.DeleteLocationAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Location not found" });
                }
                return Ok(new { message = "Location deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting location: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
