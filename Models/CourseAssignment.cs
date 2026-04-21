using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCAPulse.Models
{
    [Table("course_assignments")]
    public class CourseAssignment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("lecturer_id")]
        public int LecturerId { get; set; }

        [Required]
        [Column("course_id")]
        public int CourseId { get; set; }

        [Required]
        [Column("semester_id")]
        public int SemesterId { get; set; }

        [Required]
        [Column("group_id")]
        public int GroupId { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("LecturerId")]
        public virtual User? Lecturer { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester? Semester { get; set; }

        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }
    }
}
