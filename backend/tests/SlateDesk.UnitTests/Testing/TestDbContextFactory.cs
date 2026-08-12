using Microsoft.EntityFrameworkCore;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.UnitTests.Testing;

internal static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options =
            new DbContextOptionsBuilder<
                ApplicationDbContext>()
                .UseInMemoryDatabase(
                    Guid.NewGuid().ToString())
                .Options;

        return new ApplicationDbContext(options);
    }
}
