using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class OccupyRoomDto
    {
        [Required]
        public DateTime OccupiedUntil { get; set; }
        public string? CourseInfo { get; set; }
    }
}
