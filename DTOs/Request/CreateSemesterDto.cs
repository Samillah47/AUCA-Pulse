using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class CreateSemesterDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsCurrent { get; set; } = false;
    }
}
