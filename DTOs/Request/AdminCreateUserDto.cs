using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class AdminCreateUserDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string IdentificationNumber { get; set; } = string.Empty;

        /// <summary>STUDENT, LECTURER, STAFF, or ADMIN</summary>
        [Required]
        public string RoleType { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public int? LocationId { get; set; }
    }
}
