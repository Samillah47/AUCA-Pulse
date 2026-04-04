using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Identification number is required")]
        public string IdentificationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role type is required")]
        public string RoleType { get; set; } = string.Empty; // STUDENT, LECTURER, STAFF

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        public string? Department { get; set; }

        public int? LocationId { get; set; }
    }
}
