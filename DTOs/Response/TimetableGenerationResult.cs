namespace AUCAPulse.DTOs.Response
{
    public class TimetableGenerationResult
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public int TotalAssignments { get; set; }
        public int ScheduledCount { get; set; }
        public int UnscheduledCount { get; set; }
        public List<GeneratedScheduleEntry> Scheduled { get; set; } = new();
        public List<UnscheduledAssignment> Unscheduled { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class GeneratedScheduleEntry
    {
        public int ScheduleId { get; set; }
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
    }

    public class UnscheduledAssignment
    {
        public int AssignmentId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
