using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class UpdateSemesterDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsCurrent { get; set; }
    }
}
