using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    public enum AppointmentStatus
    {
        PENDING,
        APPROVED,
        REJECTED,
        COMPLETED,
        CANCELLED
    }

    [Table("appointments")]
    public class Appointment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("student_user_id")]
        public int StudentUserId { get; set; }

        [Required]
        [Column("staff_user_id")]
        public int StaffUserId { get; set; }

        [Required]
        [Column("appointment_date")]
        public DateTime AppointmentDate { get; set; }

        [Column("reason")]
        [MaxLength(500)]
        public string? Reason { get; set; }

        [Required]
        [Column("status")]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.PENDING;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey("StudentUserId")]
        public virtual User? StudentUser { get; set; }

        [ForeignKey("StaffUserId")]
        public virtual User? StaffUser { get; set; }
    }
}

