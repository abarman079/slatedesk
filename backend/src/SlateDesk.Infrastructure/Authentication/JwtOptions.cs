using System.ComponentModel.DataAnnotations;

namespace SlateDesk.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required]
    public string SigningKey { get; init; } = string.Empty;

    [Range(5, 60)]
    public int AccessTokenMinutes { get; init; } = 15;

    [Range(1, 30)]
    public int RefreshTokenDays { get; init; } = 7;
}