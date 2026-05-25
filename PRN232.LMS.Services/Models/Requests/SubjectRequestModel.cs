using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Services.Models.Requests
{
    public class SubjectRequestModel
    {
        [Required]
        [MaxLength(20)]
        public string SubjectCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SubjectName { get; set; } = string.Empty;

        [Required]
        public int Credit { get; set; }
    }
}
