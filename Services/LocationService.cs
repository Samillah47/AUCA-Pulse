using AUCAPulse.Data;
using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;
using Microsoft.EntityFrameworkCore;

namespace AUCAPulse.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LocationService> _logger;

        public LocationService(ApplicationDbContext context, ILogger<LocationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LocationResponse> CreateLocationAsync(CreateLocationDto request)
        {
            // Validate parent if provided
            if (request.ParentId.HasValue)
            {
                var parent = await _context.Locations.FindAsync(request.ParentId.Value);
                if (parent == null)
                {
                    throw new Exception("Parent location not found");
                }

                // Validate hierarchy
                if (!IsValidHierarchy(parent.Type, request.Type))
                {
                    throw new Exception($"Invalid hierarchy: {parent.Type} cannot have child of type {request.Type}");
                }
            }
            else if (request.Type != LocationType.PROVINCE)
            {
                throw new Exception("Only PROVINCE can be a root location");
            }

            var location = new Location
            {
                Name = request.Name,
                Code = request.Code,
                Type = request.Type,
                ParentId = request.ParentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return await GetLocationByIdAsync(location.Id) ?? throw new Exception("Failed to create location");
        }

        public async Task<LocationResponse?> GetLocationByIdAsync(int locationId)
        {
            var location = await _context.Locations
                .Include(l => l.Parent)
                .FirstOrDefaultAsync(l => l.Id == locationId);

            return location == null ? null : MapToResponse(location, false);
        }

        public async Task<List<LocationResponse>> GetAllLocationsAsync()
        {
            var locations = await _context.Locations
                .Include(l => l.Parent)
                .OrderBy(l => l.Type)
                .ThenBy(l => l.Name)
                .ToListAsync();

            return locations.Select(l => MapToResponse(l, false)).ToList();
        }

        public async Task<List<LocationResponse>> GetLocationsByTypeAsync(LocationType type)
        {
            var locations = await _context.Locations
                .Include(l => l.Parent)
                .Where(l => l.Type == type)
                .OrderBy(l => l.Name)
                .ToListAsync();

            return locations.Select(l => MapToResponse(l, false)).ToList();
        }

        public async Task<List<LocationResponse>> GetLocationsByParentIdAsync(int parentId)
        {
            var locations = await _context.Locations
                .Include(l => l.Parent)
                .Where(l => l.ParentId == parentId)
                .OrderBy(l => l.Name)
                .ToListAsync();

            return locations.Select(l => MapToResponse(l, false)).ToList();
        }

        public async Task<List<LocationResponse>> GetRootLocationsAsync()
        {
            var locations = await _context.Locations
                .Where(l => l.ParentId == null)
                .OrderBy(l => l.Name)
                .ToListAsync();

            return locations.Select(l => MapToResponse(l, true)).ToList();
        }

        public async Task<LocationResponse?> UpdateLocationAsync(int locationId, CreateLocationDto request)
        {
            var location = await _context.Locations.FindAsync(locationId);
            if (location == null) return null;

            // Validate parent if changed
            if (request.ParentId.HasValue && request.ParentId != location.ParentId)
            {
                var parent = await _context.Locations.FindAsync(request.ParentId.Value);
                if (parent == null)
                {
                    throw new Exception("Parent location not found");
                }

                if (!IsValidHierarchy(parent.Type, request.Type))
                {
                    throw new Exception($"Invalid hierarchy: {parent.Type} cannot have child of type {request.Type}");
                }

                // Prevent circular reference
                if (await IsCircularReference(locationId, request.ParentId.Value))
                {
                    throw new Exception("Circular reference detected");
                }
            }

            location.Name = request.Name;
            location.Code = request.Code;
            location.Type = request.Type;
            location.ParentId = request.ParentId;

            await _context.SaveChangesAsync();

            return await GetLocationByIdAsync(locationId);
        }

        public async Task<bool> DeleteLocationAsync(int locationId)
        {
            var location = await _context.Locations
                .Include(l => l.Children)
                .FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null) return false;

            // Check if location has children
            if (location.Children.Any())
            {
                throw new Exception("Cannot delete location with children. Delete children first.");
            }

            // Check if location is used by users
            var usersCount = await _context.Users.CountAsync(u => u.LocationId == locationId);
            if (usersCount > 0)
            {
                throw new Exception($"Cannot delete location. It is used by {usersCount} user(s).");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool IsValidHierarchy(LocationType parentType, LocationType childType)
        {
            return (parentType, childType) switch
            {
                (LocationType.PROVINCE, LocationType.DISTRICT) => true,
                (LocationType.DISTRICT, LocationType.SECTOR) => true,
                (LocationType.SECTOR, LocationType.CELL) => true,
                (LocationType.CELL, LocationType.VILLAGE) => true,
                _ => false
            };
        }

        private async Task<bool> IsCircularReference(int locationId, int newParentId)
        {
            var currentId = newParentId;
            while (currentId != 0)
            {
                if (currentId == locationId) return true;

                var parent = await _context.Locations.FindAsync(currentId);
                if (parent?.ParentId == null) break;

                currentId = parent.ParentId.Value;
            }
            return false;
        }

        private LocationResponse MapToResponse(Location location, bool includeChildren)
        {
            var response = new LocationResponse
            {
                Id = location.Id,
                Name = location.Name,
                Code = location.Code,
                Type = location.Type.ToString(),
                ParentId = location.ParentId,
                ParentName = location.Parent?.Name,
                CreatedAt = location.CreatedAt
            };

            if (includeChildren)
            {
                var children = _context.Locations
                    .Where(l => l.ParentId == location.Id)
                    .OrderBy(l => l.Name)
                    .ToList();

                response.Children = children.Select(c => MapToResponse(c, true)).ToList();
            }

            return response;
        }
    }
}
