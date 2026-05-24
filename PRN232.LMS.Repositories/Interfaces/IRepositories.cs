using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Interfaces;

public interface ISemesterRepository : IGenericRepository<Semester> { }
public interface ICourseRepository : IGenericRepository<Course> { }
public interface ISubjectRepository : IGenericRepository<Subject> { }
public interface IStudentRepository : IGenericRepository<Student> { }
public interface IEnrollmentRepository : IGenericRepository<Enrollment> { }
