using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class CreateOfficeDto
    {
        [Required]
        [MaxLength(255)]
        public string OfficeName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string OfficeNumber { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Building { get; set; }

        [MaxLength(50)]
        public string? Floor { get; set; }

        [MaxLength(255)]
        public string? Department { get; set; }

        [MaxLength(20)]
        public string? PhoneExtension { get; set; }

        public int? StaffUserId { get; set; }

        [Required]
        public AvailabilityStatus AvailabilityStatus { get; set; }

        public TimeSpan? RegularOpenTime { get; set; }

        public TimeSpan? RegularCloseTime { get; set; }
    }
}
