using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Response
{
    public class LectureScheduleResponse
    {
        public int Id { get; set; }
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public string? RoomNumber { get; set; }
        public int? SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
