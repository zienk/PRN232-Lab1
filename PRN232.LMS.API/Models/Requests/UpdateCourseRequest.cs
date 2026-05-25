using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.API.Models.Requests;

public class UpdateCourseRequest
{
    [Required]
    [MaxLength(100)]
    public string CourseName { get; set; } = string.Empty;

    [Required]
    public int SemesterId { get; set; }

    [Required]
    public int SubjectId { get; set; }
}
