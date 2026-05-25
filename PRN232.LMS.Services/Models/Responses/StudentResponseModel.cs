using System;
using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.Responses
{
    public class StudentResponseModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public List<EnrollmentResponseModel>? Enrollments { get; set; }
    }
}
