namespace PRN232.LMS.Services.Models.Responses
{
    public class CourseResponseModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public int SubjectId { get; set; }
        public SemesterResponseModel? Semester { get; set; }
        public SubjectResponseModel? Subject { get; set; }
    }
}
