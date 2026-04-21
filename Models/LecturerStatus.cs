using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    public enum Status
    {
        AVAILABLE,
        IN_CLASS,
        IN_MEETING,
        AWAY,
        UNAVAILABLE,
        AVAILABLE_FOR_APPOINTMENT
    }

    [Table("lecturer_statuses")]
    public class LecturerStatus
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("lecturer_id")]
        public int LecturerId { get; set; }

        [Required]
        [Column("status")]
        public Status Status { get; set; }

        [Column("location")]
        [MaxLength(255)]
        public string? LocationDescription { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Column("available_from")]
        public DateTime? AvailableFrom { get; set; }

        [Column("available_until")]
        public DateTime? AvailableUntil { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("LecturerId")]
        public virtual User Lecturer { get; set; } = null!;
    }
}
