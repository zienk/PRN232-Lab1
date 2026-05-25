using System;

namespace PRN232.LMS.Services.Models.Responses
{
    public class EnrollmentResponseModel
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public StudentResponseModel? Student { get; set; }
        public CourseResponseModel? Course { get; set; }
    }
}
