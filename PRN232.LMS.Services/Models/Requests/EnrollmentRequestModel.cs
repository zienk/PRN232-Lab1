using System;
using System.ComponentModel.DataAnnotations;

namespace PRN232.LMS.Services.Models.Requests
{
    public class EnrollmentRequestModel
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public DateTime EnrollDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}
