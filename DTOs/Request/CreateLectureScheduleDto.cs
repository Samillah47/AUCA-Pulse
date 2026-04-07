using System.ComponentModel.DataAnnotations;
using AUCAPulse.Models;

namespace AUCAPulse.DTOs.Request
{
    public class CreateLectureScheduleDto
    {
        [Required]
        [MaxLength(20)]
        public string DayOfWeek { get; set; } = string.Empty; // MONDAY, TUESDAY, etc.

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [MaxLength(255)]
        public string? CourseName { get; set; }

        [MaxLength(50)]
        public string? CourseCode { get; set; }

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        public int? SemesterId { get; set; }
    }
}
