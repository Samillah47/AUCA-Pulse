using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class CreateCourseAssignmentDto
    {
        [Required]
        public int LecturerId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int SemesterId { get; set; }

        [Required]
        public int GroupId { get; set; }
    }
}
