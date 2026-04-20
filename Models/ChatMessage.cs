using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    [Table("chat_messages")]
    public class ChatMessage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("sender_id")]
        public int SenderId { get; set; }

        [Required]
        [Column("receiver_id")]
        public int ReceiverId { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;

        [Column("read_at")]
        public DateTime? ReadAt { get; set; }

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;

        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; } = null!;
    }
}
