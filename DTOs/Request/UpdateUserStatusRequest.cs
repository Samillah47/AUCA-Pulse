using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class UpdateUserStatusRequest
    {
        [Required]
        public UserStatus Status { get; set; }

        public string? Reason { get; set; }
    }
}
