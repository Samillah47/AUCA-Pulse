using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class UpdateUserRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Department { get; set; }

        public int? LocationId { get; set; }
    }
}
