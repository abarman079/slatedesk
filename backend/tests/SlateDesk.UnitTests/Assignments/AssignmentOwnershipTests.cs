using SlateDesk.Application.Assignments.Models;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Assignments;
using SlateDesk.UnitTests.Testing;

namespace SlateDesk.UnitTests.Assignments;

public sealed class AssignmentOwnershipTests
{
    [Fact]
    public async Task CreateAssignment_WithoutTeacherAllocation_IsRejected()
    {
        await using var dbContext =
            TestDbContextFactory.Create();

        var academicClass =
            new AcademicClass
            {
                Name = "CSE 4A",
                Code = "CSE-4A-TEST",
                AcademicYear = "2026",
                IsActive = true
            };

        var subject =
            new Subject
            {
                Name = "Software Engineering",
                Code = "CSE-TEST",
                IsActive = true
            };

        dbContext.AcademicClasses.Add(
            academicClass);

        dbContext.Subjects.Add(subject);

        await dbContext.SaveChangesAsync();

        var service =
            new AssignmentService(
                dbContext,
                new AssignmentDeadlinePolicy());

        var request =
            new CreateAssignmentRequest
            {
                AcademicClassId =
                    academicClass.Id,
                SubjectId = subject.Id,
                Title = "Test Assignment",
                Description = "Test",
                DeadlineUtc =
                    DateTime.UtcNow.AddHours(1),
                MaximumMarks = 20
            };

        await Assert.ThrowsAsync<
            UnauthorizedAccessException>(
            () => service.CreateAssignmentAsync(
                "teacher-without-allocation",
                request,
                CancellationToken.None));
    }
}
