using Microsoft.EntityFrameworkCore;
using PRN232.LMS.Repositories.Entities;

namespace PRN232.LMS.Repositories.Data;

public class LmsDbContext : DbContext
{
    public LmsDbContext(DbContextOptions<LmsDbContext> options) : base(options) { }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Semester)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Subject)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed Data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // 5 Semesters
        var semesters = new List<Semester>();
        var semesterNames = new[] { "Fall 2024", "Spring 2025", "Summer 2025", "Fall 2025", "Spring 2026" };
        var semesterDates = new[]
        {
            (new DateTime(2024, 9, 1), new DateTime(2024, 12, 31)),
            (new DateTime(2025, 1, 15), new DateTime(2025, 5, 15)),
            (new DateTime(2025, 6, 1), new DateTime(2025, 8, 31)),
            (new DateTime(2025, 9, 1), new DateTime(2025, 12, 31)),
            (new DateTime(2026, 1, 15), new DateTime(2026, 5, 15))
        };
        for (int i = 0; i < 5; i++)
        {
            semesters.Add(new Semester
            {
                SemesterId = i + 1,
                SemesterName = semesterNames[i],
                StartDate = semesterDates[i].Item1,
                EndDate = semesterDates[i].Item2
            });
        }
        modelBuilder.Entity<Semester>().HasData(semesters);

        // 10 Subjects
        var subjectData = new[]
        {
            ("PRN211", "Basic Cross-Platform Application Programming With .NET", 3),
            ("PRN221", "Advanced Cross-Platform Application Programming With .NET", 3),
            ("PRN231", "Building Cross-Platform Back-End Application With .NET", 3),
            ("PRN232", "Building Cross-Platform Front-End Application With .NET", 3),
            ("DBI202", "Database Systems", 3),
            ("SWE201c", "Introduction to Software Engineering", 3),
            ("SWR302", "Software Requirement", 3),
            ("SWD392", "Software Architecture and Design", 3),
            ("MAS291", "Statistics and Probability", 3),
            ("MLN111", "Philosophy of Marxism – Leninism", 3)
        };
        var subjects = new List<Subject>();
        for (int i = 0; i < subjectData.Length; i++)
        {
            subjects.Add(new Subject
            {
                SubjectId = i + 1,
                SubjectCode = subjectData[i].Item1,
                SubjectName = subjectData[i].Item2,
                Credit = subjectData[i].Item3
            });
        }
        modelBuilder.Entity<Subject>().HasData(subjects);

        // 20 Courses (4 courses per semester, spread across subjects)
        var courses = new List<Course>();
        int courseId = 1;
        for (int sem = 1; sem <= 5; sem++)
        {
            for (int j = 0; j < 4; j++)
            {
                int subjectIdx = ((sem - 1) * 4 + j) % 10;
                courses.Add(new Course
                {
                    CourseId = courseId,
                    CourseName = $"{subjectData[subjectIdx].Item1}-SE{1800 + courseId}",
                    SemesterId = sem,
                    SubjectId = subjectIdx + 1
                });
                courseId++;
            }
        }
        modelBuilder.Entity<Course>().HasData(courses);

        // 50 Students
        var firstNames = new[] { "Nguyen Van", "Tran Thi", "Le Hoang", "Pham Minh", "Hoang Duc",
            "Vo Thanh", "Bui Quang", "Do Xuan", "Ngo Hai", "Dang Quoc" };
        var lastNames = new[] { "An", "Binh", "Cuong", "Dung", "Em",
            "Fong", "Giang", "Hieu", "Ich", "Khanh",
            "Linh", "Minh", "Nam", "Oanh", "Phuc",
            "Quang", "Rin", "Son", "Tuan", "Uyen",
            "Vi", "Xuan", "Yen", "Bao", "Chi" };
        var students = new List<Student>();
        var random = new Random(42); // fixed seed for reproducibility
        for (int i = 0; i < 50; i++)
        {
            var firstName = firstNames[i % firstNames.Length];
            var lastName = lastNames[i % lastNames.Length];
            students.Add(new Student
            {
                StudentId = i + 1,
                FullName = $"{firstName} {lastName}",
                Email = $"{lastName.ToLower()}{firstName.Replace(" ", "").ToLower()}{i + 1}@fpt.edu.vn",
                DateOfBirth = new DateTime(2000 + (i % 5), (i % 12) + 1, (i % 28) + 1)
            });
        }
        modelBuilder.Entity<Student>().HasData(students);

        // 500 Enrollments
        var statuses = new[] { "Active", "Completed", "Dropped", "Pending" };
        var enrollments = new List<Enrollment>();
        for (int i = 0; i < 500; i++)
        {
            int studentIdVal = (i % 50) + 1;
            int courseIdVal = (i % 20) + 1;
            var semesterForCourse = ((courseIdVal - 1) / 4) + 1;
            var semDates = semesterDates[semesterForCourse - 1];

            enrollments.Add(new Enrollment
            {
                EnrollmentId = i + 1,
                StudentId = studentIdVal,
                CourseId = courseIdVal,
                EnrollDate = semDates.Item1.AddDays(random.Next(0, 14)),
                Status = statuses[i % statuses.Length]
            });
        }
        modelBuilder.Entity<Enrollment>().HasData(enrollments);
    }
}
