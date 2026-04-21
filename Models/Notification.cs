using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    public enum NotificationType
    {
        INFO,
        SUCCESS,
        WARNING,
        ERROR,
        APPROVAL,
        REJECTION
    }

    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("title")]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Column("type")]
        public NotificationType Type { get; set; }

        /// <summary>Optional target path the UI should navigate to when the
        /// user clicks this notification (e.g. "/Appointments" or
        /// "/Student/MyAppointments").</summary>
        [Column("link")]
        [MaxLength(255)]
        public string? Link { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
