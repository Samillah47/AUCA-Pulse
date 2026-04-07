using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class CreatePasswordResetRequestDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
