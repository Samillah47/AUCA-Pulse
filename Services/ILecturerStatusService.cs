using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;
using AUCAPulse.Models;

namespace AUCAPulse.Services
{
    public interface ILecturerStatusService
    {
        Task<LecturerStatusResponse> CreateStatusAsync(int lecturerId, CreateLecturerStatusDto request);
        Task<LecturerStatusResponse?> GetStatusByIdAsync(int statusId);
        Task<List<LecturerStatusResponse>> GetAllStatusesAsync();
        Task<List<LecturerStatusResponse>> GetStatusesByLecturerIdAsync(int lecturerId);
        Task<LecturerStatusResponse?> GetCurrentStatusByLecturerIdAsync(int lecturerId);
        Task<List<LecturerStatusResponse>> GetStatusesByStatusTypeAsync(Status status);
        Task<LecturerStatusResponse?> UpdateStatusAsync(int statusId, CreateLecturerStatusDto request);
        Task<bool> DeleteStatusAsync(int statusId);
    }
}
