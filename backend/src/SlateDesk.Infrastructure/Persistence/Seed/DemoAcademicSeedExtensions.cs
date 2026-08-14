using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlateDesk.Domain.Entities;
using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Identity;

namespace SlateDesk.Infrastructure.Persistence.Seed;

public static class DemoAcademicSeedExtensions
{
    private const string DemoClassCode =
        "CSE-DEMO-2026";

    private const string DemoSubjectCode =
        "CSE-401";

    private const string DemoAssignmentTitle =
        "API Reliability Reflection";

    public static async Task
        SeedDemoAcademicDataAsync(
            this IServiceProvider services,
            IConfiguration configuration)
    {
        bool seedEnabled =
            configuration.GetValue<bool>(
                "DemoAccounts:SeedEnabled");

        if (!seedEnabled)
        {
            return;
        }

        using IServiceScope scope =
            services.CreateScope();

        ApplicationDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    ApplicationDbContext>();

        ILogger logger =
            scope.ServiceProvider
                .GetRequiredService<
                    ILoggerFactory>()
                .CreateLogger(
                    "DemoAcademicSeeder");

        string teacherEmail =
            configuration[
                "DemoAccounts:Teacher:Email"]
            ?? "teacher@slatedesk.local";

        string studentEmail =
            configuration[
                "DemoAccounts:Student:Email"]
            ?? "student@slatedesk.local";

        ApplicationUser teacher =
            await FindDemoUserAsync(
                dbContext,
                teacherEmail)
            ?? throw new InvalidOperationException(
                $"Demo Teacher '{teacherEmail}' was not found.");

        ApplicationUser student =
            await FindDemoUserAsync(
                dbContext,
                studentEmail)
            ?? throw new InvalidOperationException(
                $"Demo Student '{studentEmail}' was not found.");

        DateTime utcNow =
            DateTime.UtcNow;

        AcademicClass? demoClass =
            await dbContext.AcademicClasses
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    item =>
                        item.Code ==
                        DemoClassCode);

        if (demoClass is null)
        {
            demoClass =
                new AcademicClass
                {
                    Name =
                        "CSE Demo Cohort",
                    Code =
                        DemoClassCode,
                    AcademicYear =
                        "2026",
                    Description =
                        "Seeded SlateDesk demonstration cohort.",
                    IsActive = true,
                    CreatedAtUtc =
                        utcNow
                };

            dbContext.AcademicClasses.Add(
                demoClass);
        }
        else
        {
            demoClass.IsActive = true;
        }

        Subject? subject =
            await dbContext.Subjects
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    item =>
                        item.Code ==
                        DemoSubjectCode);

        if (subject is null)
        {
            subject =
                new Subject
                {
                    Name =
                        "Software Engineering",
                    Code =
                        DemoSubjectCode,
                    Description =
                        "Software design, reliability, validation, and testing.",
                    IsActive = true,
                    CreatedAtUtc =
                        utcNow
                };

            dbContext.Subjects.Add(
                subject);
        }
        else
        {
            subject.IsActive = true;
        }

        await dbContext.SaveChangesAsync();

        /*
         * Preserve an existing active enrollment in a
         * developer database instead of creating an
         * illegal second active enrollment.
         *
         * On a fresh clone, the demo Student is enrolled
         * in the seeded demo class.
         */
        StudentEnrollment?
            activeEnrollment =
                await dbContext
                    .StudentEnrollments
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(
                        item =>
                            item.StudentId ==
                                student.Id &&
                            item.IsActive);

        AcademicClass assignmentClass =
            demoClass;

        if (activeEnrollment is null)
        {
            StudentEnrollment?
                existingDemoEnrollment =
                    await dbContext
                        .StudentEnrollments
                        .IgnoreQueryFilters()
                        .SingleOrDefaultAsync(
                            item =>
                                item.StudentId ==
                                    student.Id &&
                                item.AcademicClassId ==
                                    demoClass.Id);

            if (existingDemoEnrollment
                is null)
            {
                dbContext
                    .StudentEnrollments
                    .Add(
                        new StudentEnrollment
                        {
                            StudentId =
                                student.Id,
                            AcademicClassId =
                                demoClass.Id,
                            EnrolledAtUtc =
                                utcNow,
                            IsActive = true
                        });
            }
            else
            {
                existingDemoEnrollment
                    .IsActive = true;
            }
        }
        else if (
            activeEnrollment
                .AcademicClassId !=
            demoClass.Id)
        {
            AcademicClass?
                existingClass =
                    await dbContext
                        .AcademicClasses
                        .IgnoreQueryFilters()
                        .SingleOrDefaultAsync(
                            item =>
                                item.Id ==
                                activeEnrollment
                                    .AcademicClassId);

            if (existingClass is not null)
            {
                assignmentClass =
                    existingClass;
            }
        }

        assignmentClass.IsActive = true;

        await dbContext.SaveChangesAsync();

        TeacherAllocation? allocation =
            await dbContext
                .TeacherAllocations
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    item =>
                        item.TeacherId ==
                            teacher.Id &&
                        item.AcademicClassId ==
                            assignmentClass.Id &&
                        item.SubjectId ==
                            subject.Id);

        if (allocation is null)
        {
            allocation =
                new TeacherAllocation
                {
                    TeacherId =
                        teacher.Id,
                    AcademicClassId =
                        assignmentClass.Id,
                    SubjectId =
                        subject.Id,
                    AssignedAtUtc =
                        utcNow,
                    IsActive = true
                };

            dbContext.TeacherAllocations.Add(
                allocation);
        }
        else
        {
            allocation.IsActive = true;
        }

        await dbContext.SaveChangesAsync();

        Assignment? assignment =
            await dbContext.Assignments
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    item =>
                        item.TeacherId ==
                            teacher.Id &&
                        item.AcademicClassId ==
                            assignmentClass.Id &&
                        item.SubjectId ==
                            subject.Id &&
                        item.Title ==
                            DemoAssignmentTitle);

        if (assignment is null)
        {
            assignment =
                new Assignment
                {
                    TeacherId =
                        teacher.Id,

                    AcademicClassId =
                        assignmentClass.Id,

                    SubjectId =
                        subject.Id,

                    Title =
                        DemoAssignmentTitle,

                    Description =
                        "Explain how validation, authorization, testing, and concurrency protection improve API reliability.",

                    Instructions =
                        "Write a concise response covering at least three reliability mechanisms.",

                    DeadlineUtc =
                        utcNow.AddDays(7),

                    MaximumMarks = 20,

                    AllowResubmission =
                        true,

                    AllowLateSubmission =
                        false,

                    Status =
                        AssignmentStatus
                            .Published,

                    PublishedAtUtc =
                        utcNow.AddDays(-2),

                    CreatedAtUtc =
                        utcNow.AddDays(-2),

                    UpdatedAtUtc =
                        utcNow,

                    IsArchived =
                        false
                };

            dbContext.Assignments.Add(
                assignment);

            await dbContext
                .SaveChangesAsync();
        }

        Submission? submission =
            await dbContext.Submissions
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    item =>
                        item.AssignmentId ==
                            assignment.Id &&
                        item.StudentId ==
                            student.Id);

        if (submission is null)
        {
            submission =
                new Submission
                {
                    AssignmentId =
                        assignment.Id,

                    StudentId =
                        student.Id,

                    AnswerText =
                        "Reliable APIs combine strict validation, backend authorization, automated testing, clear error handling, and optimistic concurrency so invalid or stale operations cannot silently corrupt application state.",

                    SubmittedAtUtc =
                        utcNow.AddDays(-1),

                    UpdatedAtUtc =
                        utcNow.AddHours(-12),

                    Status =
                        SubmissionStatus
                            .Graded,

                    MarksAwarded = 18,

                    TeacherFeedback =
                        "Clear explanation of the key reliability mechanisms. Good connection between authorization, testing, and concurrency protection.",

                    GradedAtUtc =
                        utcNow.AddHours(-12),

                    GradedByTeacherId =
                        teacher.Id
                };

            dbContext.Submissions.Add(
                submission);

            await dbContext
                .SaveChangesAsync();
        }

        logger.LogInformation(
            "SlateDesk demo academic data is ready.");
    }

    private static Task<ApplicationUser?>
        FindDemoUserAsync(
            ApplicationDbContext dbContext,
            string email)
    {
        string normalizedEmail =
            email.ToUpperInvariant();

        return dbContext.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                user =>
                    user.NormalizedEmail ==
                    normalizedEmail);
    }
}