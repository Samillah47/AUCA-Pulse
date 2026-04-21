using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    [Table("lecture_schedules")]
    public class LectureSchedule
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("lecturer_id")]
        public int LecturerId { get; set; }

        [Required]
        [Column("day_of_week")]
        [MaxLength(20)]
        public string DayOfWeek { get; set; } = string.Empty; // MONDAY, TUESDAY, etc.

        [Required]
        [Column("start_time")]
        public TimeSpan StartTime { get; set; }

        [Required]
        [Column("end_time")]
        public TimeSpan EndTime { get; set; }

        [Column("course_code")]
        [MaxLength(50)]
        public string? CourseCode { get; set; }

        [Column("course_name")]
        [MaxLength(255)]
        public string? CourseName { get; set; }

        [Column("room_number")]
        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        [Column("group_name")]
        [MaxLength(50)]
        public string? GroupName { get; set; }

        [Column("semester_id")]
        public int? SemesterId { get; set; }

        /// <summary>
        /// When the lecturer cancels today's occurrence of this weekly slot
        /// (either "suspend today" or "end now"), we stamp this column with
        /// today's date. The "is this class active right now?" query skips
        /// rows whose CancelledOn == today — so the room frees up immediately
        /// and the lecturer's live location is no longer IN_CLASS. Tomorrow
        /// the same weekly slot is active again automatically.
        /// </summary>
        [Column("cancelled_on")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancellation_reason")]
        [MaxLength(255)]
        public string? CancellationReason { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("LecturerId")]
        public virtual User Lecturer { get; set; } = null!;

        [ForeignKey("SemesterId")]
        public virtual Semester? Semester { get; set; }
    }
}
