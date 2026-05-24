using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PRN232.LMS.Repositories.Entities;

[Table("Course")]
public class Course
{
    [Key]
    public int CourseId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CourseName { get; set; } = string.Empty;

    public int SemesterId { get; set; }

    public int SubjectId { get; set; }

    // Navigation
    [ForeignKey(nameof(SemesterId))]
    public virtual Semester Semester { get; set; } = null!;

    [ForeignKey(nameof(SubjectId))]
    public virtual Subject Subject { get; set; } = null!;

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
