using System;
using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.Responses
{
    public class SemesterResponseModel
    {
        public int SemesterId { get; set; }
        public string SemesterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<CourseResponseModel>? Courses { get; set; }
    }
}
