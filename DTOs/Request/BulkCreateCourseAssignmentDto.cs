using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class BulkCreateCourseAssignmentDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public int SemesterId { get; set; }

        [Required]
        [MinLength(1)]
        public List<int> LecturerIds { get; set; } = new();
    }
}
