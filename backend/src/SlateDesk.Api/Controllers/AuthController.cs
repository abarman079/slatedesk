using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlateDesk.Application.Authentication.Interfaces;
using SlateDesk.Application.Authentication.Models;

namespace SlateDesk.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookieName =
        "slatedesk.refresh_token";

    private readonly IAuthenticationService
        _authenticationService;

    private readonly bool
        _secureRefreshCookie;

    private readonly SameSiteMode
        _refreshCookieSameSite;

    public AuthController(
        IAuthenticationService
            authenticationService,
        IConfiguration configuration)
    {
        _authenticationService =
            authenticationService;

        _secureRefreshCookie =
            configuration.GetValue(
                "AuthCookies:Secure",
                true);

        _refreshCookieSameSite =
            configuration.GetValue(
                "AuthCookies:SameSite",
                SameSiteMode.None);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(
        typeof(AuthenticationResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<AuthenticationResponse>>
        Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken)
    {
        AuthenticationSession session =
            await _authenticationService.LoginAsync(
                request,
                cancellationToken);

        WriteRefreshCookie(session);

        return Ok(session.Response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(
        typeof(AuthenticationResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<AuthenticationResponse>>
        Refresh(
            CancellationToken cancellationToken)
    {
        string? refreshToken =
            Request.Cookies[
                RefreshCookieName];

        if (string.IsNullOrWhiteSpace(
                refreshToken))
        {
            return Problem(
                type:
                    "https://slatedesk.local/errors/authentication",
                title:
                    "Authentication failed",
                statusCode:
                    StatusCodes
                        .Status401Unauthorized,
                detail:
                    "The refresh session is missing or expired.");
        }

        AuthenticationSession session =
            await _authenticationService
                .RefreshAsync(
                    refreshToken,
                    cancellationToken);

        WriteRefreshCookie(session);

        return Ok(session.Response);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        string? refreshToken =
            Request.Cookies[
                RefreshCookieName];

        await _authenticationService
            .LogoutAsync(
                refreshToken,
                cancellationToken);

        DeleteRefreshCookie();

        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(AuthenticatedUserDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<
        ActionResult<AuthenticatedUserDto>>
        GetCurrentUser(
            CancellationToken cancellationToken)
    {
        AuthenticatedUserDto user =
            await _authenticationService
                .GetCurrentUserAsync(
                    User,
                    cancellationToken);

        return Ok(user);
    }

    private void WriteRefreshCookie(
        AuthenticationSession session)
    {
        Response.Cookies.Append(
            RefreshCookieName,
            session.RefreshToken,
            CreateCookieOptions(
                session
                    .RefreshTokenExpiresAtUtc));
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(
            RefreshCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure =
                    _secureRefreshCookie,
                SameSite =
                    _refreshCookieSameSite,
                Path =
                    "/api/v1/auth"
            });
    }

    private CookieOptions
        CreateCookieOptions(
            DateTime expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure =
                _secureRefreshCookie,
            SameSite =
                _refreshCookieSameSite,
            Path =
                "/api/v1/auth",
            Expires =
                new DateTimeOffset(
                    expiresAtUtc)
        };
    }
}