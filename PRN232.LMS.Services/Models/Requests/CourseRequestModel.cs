using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Services.Models.Requests
{
    public class CourseRequestModel
    {
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        public int SemesterId { get; set; }

        [Required]
        public int SubjectId { get; set; }
    }
}
