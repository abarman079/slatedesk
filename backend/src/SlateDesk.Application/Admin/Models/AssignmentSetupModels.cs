using System.ComponentModel.DataAnnotations;

namespace SlateDesk.Application.Admin.Models;

public sealed class SetupListQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [MaxLength(100)]
    public string? Search { get; init; }

    public bool? IsActive { get; init; }
}

public sealed record TeacherAllocationDto(

Guid Id,

string TeacherId,

string TeacherName,

string TeacherEmail,

Guid AcademicClassId,

string ClassName,

string ClassCode,

Guid SubjectId,

string SubjectName,

string SubjectCode,

bool IsActive,

DateTime AssignedAtUtc);

public sealed class CreateTeacherAllocationRequest

{
    [Required]
    public string TeacherId { get; init; } =
        string.Empty;

    public Guid AcademicClassId { get; init; }

    public Guid SubjectId { get; init; }
}

public sealed record StudentEnrollmentDto(

Guid Id,

string StudentId,

string StudentName,

string StudentEmail,

Guid AcademicClassId,

string ClassName,

string ClassCode,

bool IsActive,

DateTime EnrolledAtUtc);

public sealed class CreateStudentEnrollmentRequest

{
    [Required]
    public string StudentId { get; init; } =
        string.Empty;

    public Guid AcademicClassId { get; init; }
}

public sealed class UpdateStudentEnrollmentRequest

{
    public Guid AcademicClassId { get; init; }
}

public sealed record AuditLogDto(

Guid Id,

string? UserId,

string Action,

string EntityType,

string EntityId,

string Description,

DateTime CreatedAtUtc);