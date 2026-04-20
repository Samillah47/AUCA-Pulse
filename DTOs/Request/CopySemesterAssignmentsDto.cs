using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class CopySemesterAssignmentsDto
    {
        [Required]
        public int SourceSemesterId { get; set; }

        [Required]
        public int TargetSemesterId { get; set; }
    }
}
