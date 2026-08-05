using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Identity;

namespace SlateDesk.Infrastructure.Persistence.Configurations;

internal sealed class AssignmentConfiguration
    : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("Assignments");

        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.TeacherId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(assignment => assignment.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(assignment => assignment.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(assignment => assignment.Instructions)
            .HasMaxLength(4000);

        builder.Property(assignment => assignment.MaximumMarks)
            .HasPrecision(8, 2);

        builder.Property(assignment => assignment.Status)
            .HasConversion<string>()
            .HasMaxLength(24);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(assignment => assignment.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.AcademicClass)
            .WithMany(academicClass => academicClass.Assignments)
            .HasForeignKey(assignment => assignment.AcademicClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(assignment => assignment.Subject)
            .WithMany(subject => subject.Assignments)
            .HasForeignKey(assignment => assignment.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(assignment => new
        {
            assignment.TeacherId,
            assignment.Status,
            assignment.DeadlineUtc
        });

        builder.HasIndex(assignment => new
        {
            assignment.AcademicClassId,
            assignment.Status,
            assignment.DeadlineUtc
        });

        builder.HasIndex(assignment => new
        {
            assignment.Status,
            assignment.DeadlineUtc
        });

        builder.HasQueryFilter(assignment => !assignment.IsArchived);
    }
}

internal sealed class SubmissionConfiguration
    : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");

        builder.HasKey(submission => submission.Id);

        builder.Property(submission => submission.StudentId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(submission => submission.AnswerText)
            .HasMaxLength(12000)
            .IsRequired();

        builder.Property(submission => submission.Status)
            .HasConversion<string>()
            .HasMaxLength(24);

        builder.Property(submission => submission.MarksAwarded)
            .HasPrecision(8, 2);

        builder.Property(submission => submission.TeacherFeedback)
            .HasMaxLength(4000);

        builder.Property(submission => submission.GradedByTeacherId)
            .HasMaxLength(450);

        // Npgsql maps this uint property to PostgreSQL xmin.
        builder.Property(submission => submission.Version)
            .IsRowVersion();

        builder.HasOne(submission => submission.Assignment)
            .WithMany(assignment => assignment.Submissions)
            .HasForeignKey(submission => submission.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(submission => submission.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(submission => submission.GradedByTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(submission => new
        {
            submission.AssignmentId,
            submission.StudentId
        })
        .IsUnique();

        builder.HasIndex(submission => new
        {
            submission.AssignmentId,
            submission.Status
        });

        builder.HasIndex(submission => new
        {
            submission.StudentId,
            submission.Status,
            submission.UpdatedAtUtc
        });
        builder.HasQueryFilter(
            submission => !submission.Assignment.IsArchived);
    }
}