using AUCAPulse.DTOs.Response;

namespace AUCAPulse.Services
{
    public interface ILecturerLocationService
    {
        Task<LecturerLocationResponse?> GetForLecturerAsync(int lecturerId);
        Task<List<LecturerLocationResponse>> GetForAllLecturersAsync();
    }
}
