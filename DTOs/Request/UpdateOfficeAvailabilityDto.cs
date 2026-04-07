using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class UpdateOfficeAvailabilityDto
    {
        [Required]
        public AvailabilityStatus AvailabilityStatus { get; set; }
    }
}
