using AUCAPulse.DTOs.Request;
using AUCAPulse.DTOs.Response;

namespace AUCAPulse.Services
{
    public interface ILectureScheduleService
    {
        Task<LectureScheduleResponse> CreateScheduleAsync(int lecturerId, CreateLectureScheduleDto request);
        Task<LectureScheduleResponse?> GetScheduleByIdAsync(int scheduleId);
        Task<List<LectureScheduleResponse>> GetAllSchedulesAsync();
        Task<List<LectureScheduleResponse>> GetSchedulesByLecturerIdAsync(int lecturerId);
        Task<List<LectureScheduleResponse>> GetSchedulesBySemesterIdAsync(int semesterId);
        Task<List<LectureScheduleResponse>> GetSchedulesByDayAsync(string dayOfWeek);
        Task<List<LectureScheduleResponse>> GetSchedulesByRoomAsync(string roomNumber);
        Task<LectureScheduleResponse?> UpdateScheduleAsync(int scheduleId, CreateLectureScheduleDto request);
        Task<bool> DeleteScheduleAsync(int scheduleId);
    }
}
