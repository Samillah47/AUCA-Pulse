using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;

namespace AUCAPulse.Services
{
    public interface ITimetableGeneratorService
    {
        Task<TimetableGenerationResult> GenerateAsync(GenerateTimetableDto dto);
    }
}
