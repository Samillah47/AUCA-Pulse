using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    public enum RoomType
    {
        LECTURE_HALL,
        LAB,
        MEETING_ROOM,
        OFFICE,
        LIBRARY,
        IT_LAB
    }

    public enum RoomStatus
    {
        AVAILABLE,
        OCCUPIED,
        MAINTENANCE,
        RESERVED
    }

    [Table("rooms")]
    public class Room
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("room_number")]
        [MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [Column("room_name")]
        [MaxLength(255)]
        public string RoomName { get; set; } = string.Empty;

        [Column("capacity")]
        public int? Capacity { get; set; }

        [Column("building")]
        [MaxLength(255)]
        public string? Building { get; set; }

        [Column("floor")]
        [MaxLength(50)]
        public string? Floor { get; set; }

        [Required]
        [Column("room_type")]
        public RoomType RoomType { get; set; }

        [Required]
        [Column("status")]
        public RoomStatus Status { get; set; } = RoomStatus.AVAILABLE;

        [Column("current_lecturer_id")]
        public int? CurrentLecturerId { get; set; }

        [Column("occupied_at")]
        public DateTime? OccupiedAt { get; set; }

        [Column("occupied_until")]
        public DateTime? OccupiedUntil { get; set; }

        [Column("course_info")]
        [MaxLength(255)]
        public string? CourseInfo { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("CurrentLecturerId")]
        public virtual User? CurrentLecturer { get; set; }
    }
}
