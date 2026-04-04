using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    public enum AvailabilityStatus
    {
        OPEN,
        CLOSED,
        BUSY,
        AWAY
    }

    [Table("offices")]
    public class Office
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("office_number")]
        [MaxLength(50)]
        public string OfficeNumber { get; set; } = string.Empty;

        [Required]
        [Column("office_name")]
        [MaxLength(255)]
        public string OfficeName { get; set; } = string.Empty;

        [Column("building")]
        [MaxLength(255)]
        public string? Building { get; set; }

        [Column("floor")]
        [MaxLength(50)]
        public string? Floor { get; set; }

        [Column("department")]
        [MaxLength(255)]
        public string? Department { get; set; }

        [Column("phone_extension")]
        [MaxLength(20)]
        public string? PhoneExtension { get; set; }

        [Required]
        [Column("availability_status")]
        public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.CLOSED;

        [Column("regular_open_time")]
        public TimeSpan? RegularOpenTime { get; set; }

        [Column("regular_close_time")]
        public TimeSpan? RegularCloseTime { get; set; }

        [Column("status_updated_at")]
        public DateTime? StatusUpdatedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("staff_user_id")]
        public int? StaffUserId { get; set; }

        // Navigation Properties
        [ForeignKey("StaffUserId")]
        public virtual User? StaffUser { get; set; }
    }
}
