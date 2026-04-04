using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    public enum RequestType
    {
        STUDENT,
        LECTURER,
        STAFF
    }

    public enum VerificationStatus
    {
        PENDING,
        APPROVED,
        REJECTED
    }

    [Table("verification_requests")]
    public class VerificationRequest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("submitted_id")]
        [MaxLength(50)]
        public string SubmittedId { get; set; } = string.Empty;

        [Required]
        [Column("request_type")]
        public RequestType RequestType { get; set; }

        [Required]
        [Column("status")]
        public VerificationStatus Status { get; set; } = VerificationStatus.PENDING;

        [Column("reviewed_by")]
        public int? ReviewedBy { get; set; }

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("rejection_reason")]
        public string? RejectionReason { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
