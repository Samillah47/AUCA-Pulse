namespace AUCAPulse.DTOs.Response
{
    public class CourseResponse
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Department { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
