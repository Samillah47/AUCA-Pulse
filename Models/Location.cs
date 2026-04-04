using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    public enum LocationType
    {
        PROVINCE,
        DISTRICT,
        SECTOR,
        CELL,
        VILLAGE
    }

    [Table("locations")]
    public class Location
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("code")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Column("type")]
        public LocationType Type { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ParentId")]
        public virtual Location? Parent { get; set; }

        public virtual ICollection<Location> Children { get; set; } = new List<Location>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
