using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface ILocationService
    {
        Task<LocationResponse> CreateLocationAsync(CreateLocationDto request);
        Task<LocationResponse?> GetLocationByIdAsync(int locationId);
        Task<List<LocationResponse>> GetAllLocationsAsync();
        Task<List<LocationResponse>> GetLocationsByTypeAsync(LocationType type);
        Task<List<LocationResponse>> GetLocationsByParentIdAsync(int parentId);
        Task<List<LocationResponse>> GetRootLocationsAsync();
        Task<LocationResponse?> UpdateLocationAsync(int locationId, CreateLocationDto request);
        Task<bool> DeleteLocationAsync(int locationId);
    }
}
