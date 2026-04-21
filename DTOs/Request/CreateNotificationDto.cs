using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class CreateNotificationDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        /// <summary>Optional path to navigate to when the user clicks the
        /// notification (e.g. "/Appointments").</summary>
        [MaxLength(255)]
        public string? Link { get; set; }
    }
}
