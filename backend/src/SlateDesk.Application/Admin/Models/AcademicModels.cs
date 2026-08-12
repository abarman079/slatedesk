using System.ComponentModel.DataAnnotations;

namespace SlateDesk.Application.Admin.Models;

public sealed class AcademicListQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [MaxLength(100)]
    public string? Search { get; init; }

    public bool? IsActive { get; init; }
}

public sealed record AcademicClassDto(

Guid Id,

string Name,

string Code,

string AcademicYear,

string? Description,

bool IsActive,

DateTime CreatedAtUtc);

public sealed class SaveAcademicClassRequest

{
    [Required]
    [MaxLength(120)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(40)]
    public string Code { get; init; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string AcademicYear { get; init; } =
        string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}

public sealed record SubjectDto(

Guid Id,

string Name,

string Code,

string? Description,

bool IsActive,

DateTime CreatedAtUtc);

public sealed class SaveSubjectRequest

{
    [Required]
    [MaxLength(120)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Code { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }
}

public sealed class UpdateResourceStatusRequest

{
    public bool IsActive { get; init; }
}