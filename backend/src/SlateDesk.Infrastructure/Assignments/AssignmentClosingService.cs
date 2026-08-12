using Microsoft.EntityFrameworkCore;
using SlateDesk.Application.Assignments.Interfaces;
using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.Assignments;

public sealed class AssignmentClosingService
    : IAssignmentClosingService
{
    private readonly ApplicationDbContext _dbContext;

    public AssignmentClosingService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CloseExpiredAssignmentsAsync(
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        List<Domain.Entities.Assignment>
            assignments =
                await _dbContext.Assignments
                    .Where(assignment =>
                        assignment.Status ==
                            AssignmentStatus.Published &&
                        assignment.DeadlineUtc <=
                            utcNow)
                    .ToListAsync(cancellationToken);

        foreach (Domain.Entities.Assignment assignment
                 in assignments)
        {
            assignment.Status =
                AssignmentStatus.Closed;

            assignment.UpdatedAtUtc = utcNow;
        }

        if (assignments.Count > 0)
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return assignments.Count;
    }
}
