using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SlateDesk.Application.Authentication.Interfaces;
using SlateDesk.Application.Authentication.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Identity;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.Authentication;

public sealed class AuthenticationService
    : IAuthenticationService
{
    private const string GenericLoginFailure =
        "The email address or password is incorrect.";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthenticationSession> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        string email = request.Email.Trim();

        ApplicationUser? user =
            await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            _logger.LogWarning(
                "Login failed for email {Email}.",
                email);

            throw new AuthenticationFailedException(
                GenericLoginFailure);
        }

        SignInResult signInResult =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            _logger.LogWarning(
                "Login failed for user {UserId}.",
                user.Id);

            throw new AuthenticationFailedException(
                GenericLoginFailure);
        }

        IReadOnlyCollection<string> roles =
            (await _userManager.GetRolesAsync(user))
            .ToArray();

        EnsureUserHasRole(user.Id, roles);

        AccessTokenResult accessToken =
            await _tokenService.CreateAccessTokenAsync(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                roles,
                cancellationToken);

        RefreshTokenResult refreshToken =
            _tokenService.CreateRefreshToken(
                user.Id,
                Guid.NewGuid());

        _dbContext.RefreshTokens.Add(
            refreshToken.Entity);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "User {UserId} logged in successfully.",
            user.Id);

        return CreateSession(
            user,
            roles,
            accessToken,
            refreshToken);
    }

    public async Task<AuthenticationSession> RefreshAsync(
        string rawRefreshToken,
        CancellationToken cancellationToken)
    {
        string tokenHash =
            _tokenService.HashRefreshToken(
                rawRefreshToken);

        RefreshToken? existingToken =
            await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    token => token.TokenHash == tokenHash,
                    cancellationToken);

        if (existingToken is null)
        {
            _logger.LogWarning(
                "A refresh request used an unknown token.");

            throw new AuthenticationFailedException(
                "The refresh session is invalid or expired.");
        }

        DateTime now = DateTime.UtcNow;

        bool tokenWasAlreadyUsed =
            existingToken.RevokedAtUtc is not null ||
            existingToken.ReplacedByTokenId is not null;

        if (tokenWasAlreadyUsed)
        {
            await RevokeTokenFamilyAsync(
                existingToken.FamilyId,
                "Refresh-token replay detected.",
                now,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            _logger.LogWarning(
                "Refresh-token replay detected for family {FamilyId}.",
                existingToken.FamilyId);

            throw new TokenReplayDetectedException();
        }

        if (existingToken.ExpiresAtUtc <= now)
        {
            existingToken.RevokedAtUtc = now;
            existingToken.RevokedReason = "Expired";

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            throw new AuthenticationFailedException(
                "The refresh session is invalid or expired.");
        }

        ApplicationUser? user =
            await _userManager.FindByIdAsync(
                existingToken.UserId);

        if (user is null || !user.IsActive)
        {
            await RevokeTokenFamilyAsync(
                existingToken.FamilyId,
                "User unavailable or inactive.",
                now,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            throw new AuthenticationFailedException(
                "The refresh session is invalid or expired.");
        }

        IReadOnlyCollection<string> roles =
            (await _userManager.GetRolesAsync(user))
            .ToArray();

        EnsureUserHasRole(user.Id, roles);

        AccessTokenResult accessToken =
            await _tokenService.CreateAccessTokenAsync(
                user.Id,
                user.Email ?? string.Empty,
                user.FullName,
                roles,
                cancellationToken);

        RefreshTokenResult replacementToken =
            _tokenService.CreateRefreshToken(
                user.Id,
                existingToken.FamilyId,
                existingToken.Id);

        existingToken.RevokedAtUtc = now;
        existingToken.RevokedReason = "Rotated";
        existingToken.ReplacedByTokenId =
            replacementToken.Entity.Id;

        _dbContext.RefreshTokens.Add(
            replacementToken.Entity);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Refresh token rotated for user {UserId}.",
            user.Id);

        return CreateSession(
            user,
            roles,
            accessToken,
            replacementToken);
    }

    public async Task LogoutAsync(
        string? rawRefreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            return;
        }

        string tokenHash =
            _tokenService.HashRefreshToken(
                rawRefreshToken);

        RefreshToken? token =
            await _dbContext.RefreshTokens
                .SingleOrDefaultAsync(
                    refreshToken =>
                        refreshToken.TokenHash == tokenHash,
                    cancellationToken);

        if (token is null)
        {
            return;
        }

        await RevokeTokenFamilyAsync(
            token.FamilyId,
            "Logout",
            DateTime.UtcNow,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Refresh-token family {FamilyId} was revoked during logout.",
            token.FamilyId);
    }

    public async Task<AuthenticatedUserDto> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        string? userId =
            principal.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new AuthenticationFailedException();
        }

        ApplicationUser? user =
            await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            throw new AuthenticationFailedException();
        }

        IReadOnlyCollection<string> roles =
            (await _userManager.GetRolesAsync(user))
            .ToArray();

        return new AuthenticatedUserDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            roles);
    }

    private async Task RevokeTokenFamilyAsync(
        Guid familyId,
        string reason,
        DateTime revokedAtUtc,
        CancellationToken cancellationToken)
    {
        List<RefreshToken> activeFamilyTokens =
            await _dbContext.RefreshTokens
                .Where(token =>
                    token.FamilyId == familyId &&
                    token.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

        foreach (RefreshToken token in activeFamilyTokens)
        {
            token.RevokedAtUtc = revokedAtUtc;
            token.RevokedReason = reason;
        }
    }

    private static AuthenticationSession CreateSession(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        AccessTokenResult accessToken,
        RefreshTokenResult refreshToken)
    {
        var userDto = new AuthenticatedUserDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            roles);

        var response = new AuthenticationResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            userDto);

        return new AuthenticationSession(
            response,
            refreshToken.RawToken,
            refreshToken.Entity.ExpiresAtUtc);
    }

    private static void EnsureUserHasRole(
        string userId,
        IReadOnlyCollection<string> roles)
    {
        if (roles.Count == 0)
        {
            throw new InvalidOperationException(
                $"User {userId} has no assigned role.");
        }
    }
}