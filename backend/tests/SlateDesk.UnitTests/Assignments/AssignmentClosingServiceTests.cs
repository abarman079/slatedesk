using Microsoft.EntityFrameworkCore;
using SlateDesk.Domain.Entities;
using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Assignments;
using SlateDesk.UnitTests.Testing;

namespace SlateDesk.UnitTests.Assignments;

public sealed class AssignmentClosingServiceTests
{
    [Fact]
    public async Task CloseExpiredAssignments_ClosesOnlyExpiredPublishedAssignments()
    {
        await using var dbContext =
            TestDbContextFactory.Create();

        DateTime now = DateTime.UtcNow;

        var expired = new Assignment
        {
            TeacherId = "teacher-1",
            AcademicClassId = Guid.NewGuid(),
            SubjectId = Guid.NewGuid(),
            Title = "Expired",
            Description = "Expired assignment",
            DeadlineUtc = now.AddMinutes(-5),
            MaximumMarks = 20,
            Status = AssignmentStatus.Published
        };

        var future = new Assignment
        {
            TeacherId = "teacher-1",
            AcademicClassId = Guid.NewGuid(),
            SubjectId = Guid.NewGuid(),
            Title = "Future",
            Description = "Future assignment",
            DeadlineUtc = now.AddHours(2),
            MaximumMarks = 20,
            Status = AssignmentStatus.Published
        };

        dbContext.Assignments.AddRange(
            expired,
            future);

        await dbContext.SaveChangesAsync();

        var service =
            new AssignmentClosingService(
                dbContext);

        int firstRun =
            await service
                .CloseExpiredAssignmentsAsync(
                    now,
                    CancellationToken.None);

        int secondRun =
            await service
                .CloseExpiredAssignmentsAsync(
                    now,
                    CancellationToken.None);

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun);

        Assignment storedExpired =
            await dbContext.Assignments
                .SingleAsync(
                    assignment =>
                        assignment.Id ==
                        expired.Id);

        Assignment storedFuture =
            await dbContext.Assignments
                .SingleAsync(
                    assignment =>
                        assignment.Id ==
                        future.Id);

        Assert.Equal(
            AssignmentStatus.Closed,
            storedExpired.Status);

        Assert.Equal(
            AssignmentStatus.Published,
            storedFuture.Status);
    }
}
