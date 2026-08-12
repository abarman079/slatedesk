using SlateDesk.Application.Assignments.Models;
using SlateDesk.Domain.Enums;

namespace SlateDesk.Application.Assignments.Interfaces;

public interface IAssignmentDeadlinePolicy
{
    AssignmentDeadlineDecision Evaluate(
        AssignmentStatus status,
        DateTime deadlineUtc,
        bool allowLateSubmission,
        DateTime utcNow);
}