using System.Security.Claims;
using SlateDesk.Application.Authentication.Models;

namespace SlateDesk.Application.Authentication.Interfaces;

public interface IAuthenticationService
{
    Task<AuthenticationSession> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);

    Task<AuthenticationSession> RefreshAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken);

    Task LogoutAsync(
        string? rawRefreshToken,
        CancellationToken cancellationToken);

    Task<AuthenticatedUserDto> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken);
}