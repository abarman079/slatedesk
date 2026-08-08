using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Identity;

namespace SlateDesk.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(user => user.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(user => user.IsActive);

        builder.HasQueryFilter(user => user.IsActive);
    }
}

internal sealed class AcademicClassConfiguration
    : IEntityTypeConfiguration<AcademicClass>
{
    public void Configure(EntityTypeBuilder<AcademicClass> builder)
    {
        builder.ToTable("AcademicClasses");

        builder.HasKey(academicClass => academicClass.Id);

        builder.Property(academicClass => academicClass.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(academicClass => academicClass.Code)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(academicClass => academicClass.AcademicYear)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(academicClass => academicClass.Description)
            .HasMaxLength(500);

        builder.HasIndex(academicClass => academicClass.Code)
            .IsUnique();

        builder.HasQueryFilter(academicClass => academicClass.IsActive);
    }
}

internal sealed class SubjectConfiguration
    : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");

        builder.HasKey(subject => subject.Id);

        builder.Property(subject => subject.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(subject => subject.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(subject => subject.Description)
            .HasMaxLength(500);

        builder.HasIndex(subject => subject.Code)
            .IsUnique();

        builder.HasQueryFilter(subject => subject.IsActive);
    }
}

internal sealed class TeacherAllocationConfiguration
    : IEntityTypeConfiguration<TeacherAllocation>
{
    public void Configure(EntityTypeBuilder<TeacherAllocation> builder)
    {
        builder.ToTable("TeacherAllocations");

        builder.HasKey(allocation => allocation.Id);

        builder.Property(allocation => allocation.TeacherId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(allocation => allocation.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(allocation => allocation.AcademicClass)
            .WithMany(academicClass => academicClass.TeacherAllocations)
            .HasForeignKey(allocation => allocation.AcademicClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(allocation => allocation.Subject)
            .WithMany(subject => subject.TeacherAllocations)
            .HasForeignKey(allocation => allocation.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(allocation => new
        {
            allocation.TeacherId,
            allocation.AcademicClassId,
            allocation.SubjectId
        })
            .IsUnique();

        builder.HasIndex(allocation => new
        {
            allocation.TeacherId,
            allocation.IsActive
        });

        builder.HasQueryFilter(allocation => allocation.IsActive);
    }
}

internal sealed class StudentEnrollmentConfiguration
    : IEntityTypeConfiguration<StudentEnrollment>
{
    public void Configure(EntityTypeBuilder<StudentEnrollment> builder)
    {
        builder.ToTable("StudentEnrollments");

        builder.HasKey(enrollment => enrollment.Id);

        builder.Property(enrollment => enrollment.StudentId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(enrollment => enrollment.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(enrollment => enrollment.AcademicClass)
            .WithMany(academicClass => academicClass.StudentEnrollments)
            .HasForeignKey(enrollment => enrollment.AcademicClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // A student may have only one active enrollment.
        builder.HasIndex(enrollment => enrollment.StudentId)
            .HasDatabaseName(
                "UX_StudentEnrollments_StudentId_Active")
            .IsUnique()
            .HasFilter("\"IsActive\" = TRUE");

        builder.HasIndex(enrollment => new
        {
            enrollment.AcademicClassId,
            enrollment.IsActive
        });

        builder.HasQueryFilter(enrollment => enrollment.IsActive);
    }
}