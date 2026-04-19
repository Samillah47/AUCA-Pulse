using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class CreateCourseDto
    {
        [Required]
        [MaxLength(50)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string CourseName { get; set; } = string.Empty;

        public int Credits { get; set; } = 3;

        [MaxLength(100)]
        public string? Department { get; set; }

        public string? Description { get; set; }
    }
}
