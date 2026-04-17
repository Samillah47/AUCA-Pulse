namespace AUCAPulse.DTOs.Request
{
    public class UpdateCourseDto
    {
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public int? Credits { get; set; }
        public string? Department { get; set; }
        public string? Description { get; set; }
    }
}
