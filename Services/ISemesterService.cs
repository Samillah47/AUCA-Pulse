using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;

namespace AUCAPulse.Services
{
    public interface ISemesterService
    {
        Task<SemesterResponse> CreateSemesterAsync(CreateSemesterDto dto);
        Task<SemesterResponse?> GetSemesterByIdAsync(int id);
        Task<List<SemesterResponse>> GetAllSemestersAsync();
        Task<SemesterResponse?> GetCurrentSemesterAsync();
        Task<SemesterResponse?> UpdateSemesterAsync(int id, UpdateSemesterDto dto);
        Task<bool> SetCurrentSemesterAsync(int id);
        Task<bool> DeleteSemesterAsync(int id);
    }
}
