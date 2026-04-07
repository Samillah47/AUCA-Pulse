using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class UpdatePasswordResetRequestDto
    {
        [Required]
        public RequestStatus Status { get; set; }

        public string? RejectionReason { get; set; }
    }
}
