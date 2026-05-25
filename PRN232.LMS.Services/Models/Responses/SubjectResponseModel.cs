using System.Collections.Generic;

namespace PRN232.LMS.Services.Models.Responses
{
    public class SubjectResponseModel
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credit { get; set; }
        public List<CourseResponseModel>? Courses { get; set; }
    }
}
