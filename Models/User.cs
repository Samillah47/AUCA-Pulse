using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    public enum UserStatus
    {
        PENDING,
        APPROVED,
        REJECTED
    }

    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column("identification_number")]
        [MaxLength(50)]
        public string IdentificationNumber { get; set; } = string.Empty;

        [Required]
        [Column("status")]
        public UserStatus Status { get; set; } = UserStatus.PENDING;

        [Column("phone_number")]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Column("department")]
        [MaxLength(255)]
        public string? Department { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("otp_code")]
        [MaxLength(10)]
        public string? OtpCode { get; set; }

        [Column("otp_expiry")]
        public DateTime? OtpExpiry { get; set; }

        // Staff Availability Status (for staff members only)
        [Column("availability_status")]
        public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.AWAY;

        // Foreign Keys
        [Required]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("location_id")]
        public int? LocationId { get; set; }

        // Navigation Properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey("LocationId")]
        public virtual Location? Location { get; set; }

        public virtual ICollection<LecturerStatus> LecturerStatuses { get; set; } = new List<LecturerStatus>();
        public virtual ICollection<VerificationRequest> VerificationRequests { get; set; } = new List<VerificationRequest>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<Room> OccupiedRooms { get; set; } = new List<Room>();
        public virtual ICollection<LectureSchedule> LectureSchedules { get; set; } = new List<LectureSchedule>();
        public virtual Office? Office { get; set; }
        public virtual ICollection<PasswordResetRequest> PasswordResetRequests { get; set; } = new List<PasswordResetRequest>();
        public virtual ICollection<Appointment> StudentAppointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Appointment> StaffAppointments { get; set; } = new List<Appointment>();
    }
}
