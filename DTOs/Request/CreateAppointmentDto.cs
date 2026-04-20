using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class CreateAppointmentDto
    {
        [Required]
        public int StaffUserId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}

