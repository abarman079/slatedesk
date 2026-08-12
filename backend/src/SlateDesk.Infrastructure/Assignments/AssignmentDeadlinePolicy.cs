using SlateDesk.Application.Assignments.Interfaces;
using SlateDesk.Application.Assignments.Models;
using SlateDesk.Domain.Enums;

namespace SlateDesk.Infrastructure.Assignments;

public sealed class AssignmentDeadlinePolicy
    : IAssignmentDeadlinePolicy
{
    public AssignmentDeadlineDecision Evaluate(
        AssignmentStatus status,
        DateTime deadlineUtc,
        bool allowLateSubmission,
        DateTime utcNow)
    {
        bool isPastDeadline =
            deadlineUtc <= utcNow;

        if (status is AssignmentStatus.Draft
            or AssignmentStatus.Archived)
        {
            return new AssignmentDeadlineDecision(
                isPastDeadline,
                false,
                false);
        }

        if (!isPastDeadline)
        {
            return new AssignmentDeadlineDecision(
                false,
                status == AssignmentStatus.Published,
                false);
        }

        bool canSubmitLate =
            allowLateSubmission &&
            status is AssignmentStatus.Published
                or AssignmentStatus.Closed;

        return new AssignmentDeadlineDecision(
            true,
            canSubmitLate,
            canSubmitLate);
    }
}