using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Identity;

namespace SlateDesk.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AcademicClass> AcademicClasses => Set<AcademicClass>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<TeacherAllocation> TeacherAllocations
        => Set<TeacherAllocation>();

    public DbSet<StudentEnrollment> StudentEnrollments
        => Set<StudentEnrollment>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}