namespace PRN232.LMS.Services.BusinessModels;

public class CourseModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }
    public SemesterModel? Semester { get; set; }
    public SubjectModel? Subject { get; set; }
    public List<EnrollmentModel>? Enrollments { get; set; }
}
