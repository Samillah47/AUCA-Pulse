using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class CreateVerificationRequestDto
    {
        [Required]
        [MaxLength(50)]
        public string SubmittedId { get; set; } = string.Empty;

        [Required]
        public RequestType RequestType { get; set; }
    }
}
