using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SlateDesk.Application.Authentication.Interfaces;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Authentication;
using SlateDesk.Infrastructure.Identity;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.UnitTests.Authentication;

public sealed class RefreshTokenReplayTests
{
    [Fact]
    public async Task ReusingRotatedToken_RevokesActiveTokenFamily()
    {
        await using ServiceProvider provider =
            CreateProvider();

        using IServiceScope scope =
            provider.CreateScope();

        ApplicationDbContext dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    ApplicationDbContext>();

        ITokenService tokenService =
            scope.ServiceProvider
                .GetRequiredService<
                    ITokenService>();

        AuthenticationService authService =
            scope.ServiceProvider
                .GetRequiredService<
                    AuthenticationService>();

        Guid familyId = Guid.NewGuid();

        string oldRawToken =
            "old-refresh-token-for-test";

        var oldToken =
            new RefreshToken
            {
                UserId = "user-1",
                TokenHash =
                    tokenService.HashRefreshToken(
                        oldRawToken),
                FamilyId = familyId,
                CreatedAtUtc =
                    DateTime.UtcNow.AddMinutes(-5),
                ExpiresAtUtc =
                    DateTime.UtcNow.AddDays(1),
                RevokedAtUtc =
                    DateTime.UtcNow.AddMinutes(-1),
                ReplacedByTokenId =
                    Guid.NewGuid(),
                RevokedReason = "Rotated"
            };

        var activeToken =
            new RefreshToken
            {
                UserId = "user-1",
                TokenHash =
                    tokenService.HashRefreshToken(
                        "active-refresh-token"),
                FamilyId = familyId,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc =
                    DateTime.UtcNow.AddDays(1)
            };

        dbContext.RefreshTokens.AddRange(
            oldToken,
            activeToken);

        await dbContext.SaveChangesAsync();

        await Assert.ThrowsAsync<
            TokenReplayDetectedException>(
            () => authService.RefreshAsync(
                oldRawToken,
                CancellationToken.None));

        RefreshToken storedActiveToken =
            await dbContext.RefreshTokens
                .SingleAsync(
                    token =>
                        token.Id ==
                        activeToken.Id);

        Assert.NotNull(
            storedActiveToken.RevokedAtUtc);

        Assert.Equal(
            "Refresh-token replay detected.",
            storedActiveToken.RevokedReason);
    }

    private static ServiceProvider
        CreateProvider()
    {
        var services =
            new ServiceCollection();

        services.AddLogging();
        services.AddAuthentication();
        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddDataProtection();

        services.AddDbContext<
            ApplicationDbContext>(
            options =>
                options.UseInMemoryDatabase(
                    Guid.NewGuid().ToString()));

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<
                ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton<
            IOptions<JwtOptions>>(
            Options.Create(
                new JwtOptions
                {
                    Issuer =
                        "SlateDesk.Tests",
                    Audience =
                        "SlateDesk.Tests",
                    SigningKey =
                        new string('S', 64),
                    AccessTokenMinutes = 15,
                    RefreshTokenDays = 7
                }));

        services.AddScoped<
            ITokenService,
            JwtTokenService>();

        services.AddScoped<
            AuthenticationService>();

        return services.BuildServiceProvider();
    }
}
