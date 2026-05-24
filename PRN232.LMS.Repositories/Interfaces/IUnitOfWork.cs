namespace PRN232.LMS.Repositories.Interfaces;

/// <summary>
/// Unit of Work interface — coordinates the work of multiple repositories
/// by sharing a single DbContext and managing transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ISemesterRepository Semesters { get; }
    ICourseRepository Courses { get; }
    ISubjectRepository Subjects { get; }
    IStudentRepository Students { get; }
    IEnrollmentRepository Enrollments { get; }

    /// <summary>
    /// Persists all changes made through the repositories in a single transaction.
    /// </summary>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync();
}
