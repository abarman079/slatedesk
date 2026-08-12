using System.ComponentModel.DataAnnotations;

namespace SlateDesk.Application.Admin.Models;

public sealed record AdminUserDto(
    string Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed class AdminUserQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 10;

    [MaxLength(100)]
    public string? Search { get; init; }

    [MaxLength(20)]
    public string? Role { get; init; }

    public bool? IsActive { get; init; }
}

public sealed class CreateAdminUserRequest

{
    [Required]
    [MaxLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Role { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}

public sealed class UpdateAdminUserRequest

{
    [Required]
    [MaxLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; init; } = string.Empty;
}

public sealed class UpdateUserStatusRequest

{
    public bool IsActive { get; init; }
}

public sealed class ResetUserPasswordRequest

{
    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string NewPassword { get; init; } = string.Empty;
}