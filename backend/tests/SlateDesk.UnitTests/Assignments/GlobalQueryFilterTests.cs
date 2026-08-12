using Microsoft.EntityFrameworkCore;
using SlateDesk.Domain.Entities;
using SlateDesk.UnitTests.Testing;

namespace SlateDesk.UnitTests.Assignments;

public sealed class GlobalQueryFilterTests
{
    [Fact]
    public async Task AcademicClasses_DefaultQuery_ExcludesInactiveRecords()
    {
        await using var dbContext =
            TestDbContextFactory.Create();

        dbContext.AcademicClasses.AddRange(
            new AcademicClass
            {
                Name = "Active Class",
                Code = "ACTIVE-1",
                AcademicYear = "2026",
                IsActive = true
            },
            new AcademicClass
            {
                Name = "Inactive Class",
                Code = "INACTIVE-1",
                AcademicYear = "2026",
                IsActive = false
            });

        await dbContext.SaveChangesAsync();

        int normalCount =
            await dbContext.AcademicClasses.CountAsync();

        int administrativeCount =
            await dbContext.AcademicClasses
                .IgnoreQueryFilters()
                .CountAsync();

        Assert.Equal(1, normalCount);
        Assert.Equal(2, administrativeCount);
    }
}
