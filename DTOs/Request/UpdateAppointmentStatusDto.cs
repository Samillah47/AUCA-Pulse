using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class UpdateAppointmentStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}

