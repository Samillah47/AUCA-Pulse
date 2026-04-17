using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    [Table("courses")]
    public class Course
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("course_code")]
        [MaxLength(50)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [Column("course_name")]
        [MaxLength(255)]
        public string CourseName { get; set; } = string.Empty;

        [Column("credits")]
        public int Credits { get; set; } = 3;

        [Column("department")]
        [MaxLength(100)]
        public string? Department { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<CourseAssignment> CourseAssignments { get; set; } = new List<CourseAssignment>();
    }
}
