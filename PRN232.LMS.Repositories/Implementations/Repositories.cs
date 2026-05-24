using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Entities;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
{
    public SemesterRepository(LmsDbContext context) : base(context) { }
}

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(LmsDbContext context) : base(context) { }
}

public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
{
    public SubjectRepository(LmsDbContext context) : base(context) { }
}

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(LmsDbContext context) : base(context) { }
}

public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(LmsDbContext context) : base(context) { }
}
