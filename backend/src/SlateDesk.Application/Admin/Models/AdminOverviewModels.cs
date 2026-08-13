using System.ComponentModel.DataAnnotations;
using SlateDesk.Domain.Enums;

namespace SlateDesk.Application.Admin.Models;

public sealed class AdminAssignmentOverviewQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;

    [MaxLength(100)]
    public string? Search { get; init; }

    public AssignmentStatus? Status { get; init; }
}

public sealed class AdminSubmissionOverviewQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;

    [MaxLength(100)]
    public string? Search { get; init; }

    public SubmissionStatus? Status { get; init; }
}

public sealed record AdminDashboardDto(
    int ActiveTeachers,
    int ActiveStudents,
    int ActiveClasses,
    int ActiveSubjects,
    int PublishedAssignments,
    int TotalSubmissions,
    IReadOnlyCollection<AuditLogDto> RecentActivity);

public sealed record AdminAssignmentOverviewDto(
    Guid Id,
    string Title,
    string TeacherName,
    string ClassName,
    string ClassCode,
    string SubjectName,
    string SubjectCode,
    DateTime DeadlineUtc,
    decimal MaximumMarks,
    AssignmentStatus Status,
    int SubmissionCount,
    bool IsArchived);

public sealed record AdminSubmissionOverviewDto(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    string StudentName,
    string TeacherName,
    DateTime? SubmittedAtUtc,
    SubmissionStatus Status,
    decimal? MarksAwarded,
    decimal MaximumMarks,
    DateTime? GradedAtUtc);

public sealed record AppSettingDto(
    Guid Id,
    string Key,
    string Value,
    string? Description,
    DateTime UpdatedAtUtc);

public sealed class UpdateAppSettingRequest
{
    [Required]
    [MaxLength(500)]
    public string Value { get; init; } = string.Empty;
}