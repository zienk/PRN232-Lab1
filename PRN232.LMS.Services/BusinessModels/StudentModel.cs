namespace PRN232.LMS.Services.BusinessModels;

public class StudentModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentModel>? Enrollments { get; set; }
}
