using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SlateDesk.Application.Authentication.Interfaces;
using SlateDesk.Application.Authentication.Models;
using SlateDesk.Domain.Entities;

namespace SlateDesk.Infrastructure.Authentication;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public Task<AccessTokenResult> CreateAccessTokenAsync(
        string userId,
        string email,
        string fullName,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;

        DateTime expiresAtUtc =
            now.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, fullName),
            new(ClaimTypes.Email, email)
        };

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey));

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        string serializedToken =
            new JwtSecurityTokenHandler().WriteToken(token);

        return Task.FromResult(
            new AccessTokenResult(
                serializedToken,
                expiresAtUtc));
    }

    public RefreshTokenResult CreateRefreshToken(
        string userId,
        Guid familyId,
        Guid? parentTokenId = null)
    {
        byte[] randomBytes =
            RandomNumberGenerator.GetBytes(64);

        string rawToken =
            WebEncoders.Base64UrlEncode(randomBytes);

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = HashRefreshToken(rawToken),
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(
                _options.RefreshTokenDays),
            FamilyId = familyId,
            ParentTokenId = parentTokenId
        };

        return new RefreshTokenResult(
            rawToken,
            entity);
    }

    public string HashRefreshToken(string rawToken)
    {
        byte[] tokenBytes =
            Encoding.UTF8.GetBytes(rawToken);

        byte[] hash =
            SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hash);
    }
}