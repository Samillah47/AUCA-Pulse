using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class CreateLecturerStatusDto
    {
        [Required]
        public Status Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
