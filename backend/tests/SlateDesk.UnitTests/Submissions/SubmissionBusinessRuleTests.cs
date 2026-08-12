using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Submissions.Models;
using SlateDesk.Domain.Entities;
using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Assignments;
using SlateDesk.Infrastructure.Submissions;
using SlateDesk.UnitTests.Testing;

namespace SlateDesk.UnitTests.Submissions;

public sealed class SubmissionBusinessRuleTests
{
    [Fact]
    public async Task SaveDraft_ForStudentFromDifferentClass_ReturnsNotFound()
    {
        await using var dbContext =
            TestDbContextFactory.Create();

        var assignedClass =
            new AcademicClass
            {
                Name = "Assigned",
                Code = "ASSIGNED",
                AcademicYear = "2026"
            };

        var otherClass =
            new AcademicClass
            {
                Name = "Other",
                Code = "OTHER",
                AcademicYear = "2026"
            };

        var subject =
            new Subject
            {
                Name = "Software Engineering",
                Code = "SE"
            };

        dbContext.AddRange(
            assignedClass,
            otherClass,
            subject);

        var assignment =
            new Assignment
            {
                TeacherId = "teacher-1",
                AcademicClass = assignedClass,
                Subject = subject,
                Title = "Protected Assignment",
                Description = "Test",
                DeadlineUtc =
                    DateTime.UtcNow.AddHours(1),
                MaximumMarks = 30,
                Status =
                    AssignmentStatus.Published
            };

        dbContext.Assignments.Add(
            assignment);

        dbContext.StudentEnrollments.Add(
            new StudentEnrollment
            {
                StudentId = "student-2",
                AcademicClass = otherClass,
                IsActive = true
            });

        await dbContext.SaveChangesAsync();

        var service =
            new SubmissionService(
                dbContext,
                new AssignmentDeadlinePolicy());

        await Assert.ThrowsAsync<
            ResourceNotFoundException>(
            () => service.SaveDraftAsync(
                assignment.Id,
                "student-2",
                new SaveSubmissionDraftRequest
                {
                    AnswerText = "Attempt"
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task Grade_WhenMarksExceedMaximum_IsRejected()
    {
        await using var dbContext =
            TestDbContextFactory.Create();

        var assignment =
            new Assignment
            {
                TeacherId = "teacher-1",
                AcademicClassId =
                    Guid.NewGuid(),
                SubjectId =
                    Guid.NewGuid(),
                Title = "Marks Test",
                Description = "Test",
                DeadlineUtc =
                    DateTime.UtcNow.AddHours(1),
                MaximumMarks = 30,
                Status =
                    AssignmentStatus.Published
            };

        var submission =
            new Submission
            {
                Assignment = assignment,
                StudentId = "student-1",
                AnswerText = "Answer",
                SubmittedAtUtc =
                    DateTime.UtcNow,
                UpdatedAtUtc =
                    DateTime.UtcNow,
                Status =
                    SubmissionStatus.Submitted
            };

        dbContext.Submissions.Add(
            submission);

        await dbContext.SaveChangesAsync();

        var service =
            new SubmissionService(
                dbContext,
                new AssignmentDeadlinePolicy());

        await Assert.ThrowsAsync<
            BusinessRuleException>(
            () => service.GradeAsync(
                submission.Id,
                "teacher-1",
                new GradeSubmissionRequest
                {
                    MarksAwarded = 31,
                    Version = 1
                },
                CancellationToken.None));
    }

    [Fact]
    public async Task Submit_WhenResubmissionDisabled_IsRejected()
    {
        await using var dbContext =
            TestDbContextFactory.Create();

        var assignment =
            new Assignment
            {
                TeacherId = "teacher-1",
                AcademicClassId =
                    Guid.NewGuid(),
                SubjectId =
                    Guid.NewGuid(),
                Title = "No Resubmit",
                Description = "Test",
                DeadlineUtc =
                    DateTime.UtcNow.AddHours(1),
                MaximumMarks = 30,
                AllowResubmission = false,
                Status =
                    AssignmentStatus.Published
            };

        var submission =
            new Submission
            {
                Assignment = assignment,
                StudentId = "student-1",
                AnswerText =
                    "Already submitted",
                SubmittedAtUtc =
                    DateTime.UtcNow.AddMinutes(-5),
                UpdatedAtUtc =
                    DateTime.UtcNow,
                Status =
                    SubmissionStatus.Submitted
            };

        dbContext.Submissions.Add(
            submission);

        await dbContext.SaveChangesAsync();

        var service =
            new SubmissionService(
                dbContext,
                new AssignmentDeadlinePolicy());

        await Assert.ThrowsAsync<
            BusinessRuleException>(
            () => service.SubmitAsync(
                submission.Id,
                "student-1",
                new SubmitSubmissionRequest
                {
                    Version = 1
                },
                CancellationToken.None));
    }
}
