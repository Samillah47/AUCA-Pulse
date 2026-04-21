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

        /// <summary>Cancel the occurrence of a weekly schedule for today only.
        /// Sets CancelledOn to today's date so the class is treated as not
        /// active until midnight. Next week's occurrence is unaffected. Also
        /// releases the room if the lecturer was holding it.</summary>
        Task<LectureScheduleResponse?> CancelForTodayAsync(int scheduleId, int lecturerId, string? reason);

        /// <summary>Clear a previous cancellation for today.</summary>
        Task<LectureScheduleResponse?> ReinstateForTodayAsync(int scheduleId, int lecturerId);

        /// <summary>List every schedule row whose CancelledOn falls on the
        /// given date (defaults to today). Used by the admin "cancelled
        /// classes" dashboard.</summary>
        Task<List<LectureScheduleResponse>> GetCancelledClassesAsync(DateTime? date = null);
    }
}
