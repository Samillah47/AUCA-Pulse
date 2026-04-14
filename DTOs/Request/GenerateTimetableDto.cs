using System.ComponentModel.DataAnnotations;

namespace AUCAPulse.DTOs.Request
{
    public class GenerateTimetableDto
    {
        [Required]
        public int SemesterId { get; set; }

        // Daily window in hours (24h clock). Defaults to 8 AM - 6 PM.
        public int StartHour { get; set; } = 8;
        public int EndHour { get; set; } = 18;

        // Duration of each lecture slot in minutes.
        public int SlotDurationMinutes { get; set; } = 120;

        // Whether to clear any existing schedules for the semester before generating.
        public bool ReplaceExisting { get; set; } = true;
    }
}
