using System.ComponentModel.DataAnnotations;
using SlateDesk.Domain.Enums;

namespace SlateDesk.Application.Submissions.Models;

public sealed class SubmissionListQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    public SubmissionStatus? Status { get; init; }
}

public sealed class SaveSubmissionDraftRequest
{
    [MaxLength(12000)]
    public string AnswerText { get; init; } = string.Empty;
}

public sealed class UpdateSubmissionRequest
{
    [MaxLength(12000)]
    public string AnswerText { get; init; } = string.Empty;

    public uint Version { get; init; }
}

public sealed class SubmitSubmissionRequest
{
    public uint Version { get; init; }
}

public sealed class UpdateReviewStatusRequest
{
    public SubmissionStatus Status { get; init; }

    public uint Version { get; init; }
}

public sealed class GradeSubmissionRequest
{
    [Range(typeof(decimal), "0", "1000000")]
    public decimal MarksAwarded { get; init; }

    [MaxLength(4000)]
    public string? TeacherFeedback { get; init; }

    public uint Version { get; init; }
}

public sealed record StudentSubmissionDto(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    string SubjectCode,
    DateTime DeadlineUtc,
    decimal MaximumMarks,
    string AnswerText,
    DateTime? SubmittedAtUtc,
    DateTime UpdatedAtUtc,
    SubmissionStatus Status,
    decimal? MarksAwarded,
    string? TeacherFeedback,
    DateTime? GradedAtUtc,
    uint Version,
    bool CanEdit,
    bool CanSubmit);

public sealed record TeacherSubmissionDto(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    string StudentId,
    string StudentName,
    string StudentEmail,
    string AnswerText,
    DateTime? SubmittedAtUtc,
    DateTime UpdatedAtUtc,
    SubmissionStatus Status,
    decimal? MarksAwarded,
    string? TeacherFeedback,
    DateTime? GradedAtUtc,
    uint Version,
    bool IsLate);

public sealed record StudentResultDto(
    Guid SubmissionId,
    Guid AssignmentId,
    string AssignmentTitle,
    string SubjectName,
    string SubjectCode,
    decimal MarksAwarded,
    decimal MaximumMarks,
    string? TeacherFeedback,
    DateTime? GradedAtUtc,
    SubmissionStatus Status);
