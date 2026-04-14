namespace AUCAPulse.DTOs.Response
{
    public class CourseAssignmentResponse
    {
        public int Id { get; set; }
        public int LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}
