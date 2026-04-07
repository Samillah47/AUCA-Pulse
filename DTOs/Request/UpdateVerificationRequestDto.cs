using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class UpdateVerificationRequestDto
    {
        [Required]
        public VerificationStatus Status { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}
