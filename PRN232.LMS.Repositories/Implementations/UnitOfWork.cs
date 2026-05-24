using Microsoft.EntityFrameworkCore.Storage;
using PRN232.LMS.Repositories.Data;
using PRN232.LMS.Repositories.Interfaces;

namespace PRN232.LMS.Repositories.Implementations;

/// <summary>
/// Unit of Work implementation — shares a single DbContext across all repositories
/// and manages transaction lifetime.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LmsDbContext _context;
    private IDbContextTransaction? _transaction;

    private ISemesterRepository? _semesters;
    private ICourseRepository? _courses;
    private ISubjectRepository? _subjects;
    private IStudentRepository? _students;
    private IEnrollmentRepository? _enrollments;

    public UnitOfWork(LmsDbContext context)
    {
        _context = context;
    }

    public ISemesterRepository Semesters =>
        _semesters ??= new SemesterRepository(_context);

    public ICourseRepository Courses =>
        _courses ??= new CourseRepository(_context);

    public ISubjectRepository Subjects =>
        _subjects ??= new SubjectRepository(_context);

    public IStudentRepository Students =>
        _students ??= new StudentRepository(_context);

    public IEnrollmentRepository Enrollments =>
        _enrollments ??= new EnrollmentRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
