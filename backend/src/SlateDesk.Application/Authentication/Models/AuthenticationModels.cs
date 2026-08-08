using SlateDesk.Domain.Entities;

namespace SlateDesk.Application.Authentication.Models;

public sealed record AuthenticatedUserDto(
    string Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles);

public sealed record AuthenticationResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    AuthenticatedUserDto User);

public sealed record AuthenticationSession(
    AuthenticationResponse Response,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);

public sealed record RefreshTokenResult(
    string RawToken,
    RefreshToken Entity);