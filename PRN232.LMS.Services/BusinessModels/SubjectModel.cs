namespace PRN232.LMS.Services.BusinessModels;

public class SubjectModel
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public List<CourseModel>? Courses { get; set; }
}
