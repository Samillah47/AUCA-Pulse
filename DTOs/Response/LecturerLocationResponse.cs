namespace AUCAPulse.DTOs.Response
{
    /// <summary>
    /// Combined real-time view of a lecturer's whereabouts used by the student dashboard.
    /// Computed by merging current LectureSchedule, latest LecturerStatus, and Office.
    /// </summary>
    public class LecturerLocationResponse
    {
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public string? Department { get; set; }

        // One of: IN_CLASS, IN_OFFICE, AVAILABLE, IN_MEETING, AWAY, UNAVAILABLE, UNKNOWN
        public string Status { get; set; } = "UNKNOWN";

        // Human-readable location, e.g. "Room A-101", "Office B-102", or free-text note
        public string LocationLabel { get; set; } = string.Empty;

        // Optional extras for UI
        public string? CourseInfo { get; set; }       // e.g. "CS301 — Data Structures"
        public string? UntilTime { get; set; }        // e.g. "10:00"
        public string? Source { get; set; }           // "schedule" | "status" | "office" | "none"
    }
}
