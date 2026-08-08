using SlateDesk.Application.Authentication.Models;

namespace SlateDesk.Application.Authentication.Interfaces;

public interface ITokenService
{
    Task<AccessTokenResult> CreateAccessTokenAsync(
        string userId,
        string email,
        string fullName,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken);

    RefreshTokenResult CreateRefreshToken(
        string userId,
        Guid familyId,
        Guid? parentTokenId = null);

    string HashRefreshToken(string rawToken);
}