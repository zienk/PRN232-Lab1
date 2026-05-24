namespace PRN232.LMS.API.Models.Responses;

// ===== Semester Response =====
public class SemesterResponse
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<CourseResponse>? Courses { get; set; }
}

// ===== Course Response =====
public class CourseResponse
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }
    public SemesterResponse? Semester { get; set; }
    public SubjectResponse? Subject { get; set; }
}

// ===== Subject Response =====
public class SubjectResponse
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public List<CourseResponse>? Courses { get; set; }
}

// ===== Student Response =====
public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<EnrollmentResponse>? Enrollments { get; set; }
}

// ===== Enrollment Response =====
public class EnrollmentResponse
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public StudentResponse? Student { get; set; }
    public CourseResponse? Course { get; set; }
}
