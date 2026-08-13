using System.ComponentModel.DataAnnotations;
using SlateDesk.Domain.Enums;

namespace SlateDesk.Application.Assignments.Models;

public sealed class TeacherAssignmentListQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [MaxLength(100)]
    public string? Search { get; init; }

    public AssignmentStatus? Status { get; init; }
}

public sealed class StudentAssignmentListQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [MaxLength(100)]
    public string? Search { get; init; }
}

public sealed class CreateAssignmentRequest
{
    public Guid AcademicClassId { get; init; }

    public Guid SubjectId { get; init; }

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [MaxLength(4000)]
    public string? Instructions { get; init; }

    public DateTime DeadlineUtc { get; init; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal MaximumMarks { get; init; }

    public bool AllowResubmission { get; init; }

    public bool AllowLateSubmission { get; init; }
}

public sealed class UpdateAssignmentRequest
{
    public Guid AcademicClassId { get; init; }

    public Guid SubjectId { get; init; }

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [MaxLength(4000)]
    public string? Instructions { get; init; }

    public DateTime DeadlineUtc { get; init; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal MaximumMarks { get; init; }

    public bool AllowResubmission { get; init; }

    public bool AllowLateSubmission { get; init; }
}

public sealed record TeacherAssignmentDto(
    Guid Id,
    Guid AcademicClassId,
    string ClassName,
    string ClassCode,
    Guid SubjectId,
    string SubjectName,
    string SubjectCode,
    string Title,
    string Description,
    string? Instructions,
    DateTime DeadlineUtc,
    decimal MaximumMarks,
    bool AllowResubmission,
    bool AllowLateSubmission,
    AssignmentStatus Status,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    int SubmissionCount,
    bool IsPastDeadline);

public sealed record TeacherAllocationOptionDto(
    Guid AcademicClassId,
    string ClassName,
    string ClassCode,
    Guid SubjectId,
    string SubjectName,
    string SubjectCode);

public sealed record StudentAssignmentDto(
    Guid Id,
    string TeacherName,
    Guid AcademicClassId,
    string ClassName,
    string ClassCode,
    Guid SubjectId,
    string SubjectName,
    string SubjectCode,
    string Title,
    string Description,
    string? Instructions,
    DateTime DeadlineUtc,
    decimal MaximumMarks,
    bool AllowResubmission,
    bool AllowLateSubmission,
    AssignmentStatus Status,
    SubmissionStatus? SubmissionStatus,
    bool IsPastDeadline,
    bool CanSubmit,
    bool WouldBeLate);

public sealed record AssignmentDeadlineDecision(
    bool IsPastDeadline,
    bool CanSubmit,
    bool WouldBeLate);