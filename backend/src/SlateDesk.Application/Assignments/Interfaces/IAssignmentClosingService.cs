namespace SlateDesk.Application.Assignments.Interfaces;

public interface IAssignmentClosingService
{
    Task<int> CloseExpiredAssignmentsAsync(
        DateTime utcNow,
        CancellationToken cancellationToken);
}
