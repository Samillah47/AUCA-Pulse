using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface IOfficeService
    {
        Task<OfficeResponse> CreateOfficeAsync(CreateOfficeDto request);
        Task<OfficeResponse?> GetOfficeByIdAsync(int officeId);
        Task<List<OfficeResponse>> GetAllOfficesAsync();
        Task<List<OfficeResponse>> GetOfficesByStatusAsync(AvailabilityStatus status);
        Task<OfficeResponse?> GetOfficeByUserIdAsync(int staffUserId);
        Task<OfficeResponse?> UpdateOfficeAsync(int officeId, CreateOfficeDto request);
        Task<OfficeResponse?> UpdateOfficeAvailabilityAsync(int officeId, UpdateOfficeAvailabilityDto request);
        Task<bool> DeleteOfficeAsync(int officeId);
    }
}
