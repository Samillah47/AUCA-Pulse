namespace AUCAPulse.DTOs.Response
{
    public class BulkCourseAssignmentResult
    {
        public int Created { get; set; }
        public int SkippedDuplicates { get; set; }
        public int InvalidLecturers { get; set; }
        public List<CourseAssignmentResponse> Assignments { get; set; } = new();
        public List<string> Notes { get; set; } = new();
    }
}
