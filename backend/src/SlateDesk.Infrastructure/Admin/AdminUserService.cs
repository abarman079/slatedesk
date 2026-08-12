using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SlateDesk.Application.Admin.Interfaces;
using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Domain.Constants;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Identity;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.Admin;

public sealed class AdminUserService
    : IAdminUserService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<PagedResult<AdminUserDto>>
        GetUsersAsync(
            AdminUserQuery query,
            CancellationToken cancellationToken)
    {
        IQueryable<ApplicationUser> users =
            _dbContext.Users
                .IgnoreQueryFilters()
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            users = users.Where(user =>
                user.FullName.ToLower().Contains(search) ||
                (user.Email != null &&
                 user.Email.ToLower().Contains(search)));
        }

        if (query.IsActive.HasValue)
        {
            users = users.Where(
                user =>
                    user.IsActive ==
                    query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            string role = query.Role.Trim();

            users = users.Where(user =>
                _dbContext.UserRoles.Any(userRole =>
                    userRole.UserId == user.Id &&
                    _dbContext.Roles.Any(identityRole =>
                        identityRole.Id == userRole.RoleId &&
                        identityRole.Name == role)));
        }

        int totalItems =
            await users.CountAsync(
                cancellationToken);

        var pageUsers =
            await users
                .OrderBy(user => user.FullName)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(user => new
                {
                    user.Id,
                    user.FullName,
                    Email = user.Email ?? string.Empty,
                    user.IsActive,
                    user.CreatedAtUtc,
                    user.UpdatedAtUtc
                })
                .ToListAsync(cancellationToken);

        string[] ids =
            pageUsers
                .Select(user => user.Id)
                .ToArray();

        var roleRows =
            await (
                from userRole in _dbContext.UserRoles
                join role in _dbContext.Roles
                    on userRole.RoleId equals role.Id
                where ids.Contains(userRole.UserId)
                select new
                {
                    userRole.UserId,
                    RoleName =
                        role.Name ?? string.Empty
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        Dictionary<string, string[]> rolesByUser =
            roleRows
                .GroupBy(row => row.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(row => row.RoleName)
                        .OrderBy(role => role)
                        .ToArray());

        AdminUserDto[] items =
            pageUsers
                .Select(user =>
                    new AdminUserDto(
                        user.Id,
                        user.FullName,
                        user.Email,
                        rolesByUser.GetValueOrDefault(
                            user.Id,
                            []),
                        user.IsActive,
                        user.CreatedAtUtc,
                        user.UpdatedAtUtc))
                .ToArray();

        return PagedResult<AdminUserDto>.Create(
            items,
            query.Page,
            query.PageSize,
            totalItems);
    }

    public async Task<AdminUserDto> GetUserAsync(
        string id,
        CancellationToken cancellationToken)
    {
        ApplicationUser user =
            await GetUserIncludingInactiveAsync(
                id,
                cancellationToken);

        IReadOnlyCollection<string> roles =
            (await _userManager.GetRolesAsync(user))
            .ToArray();

        return MapUser(user, roles);
    }

    public async Task<AdminUserDto> CreateUserAsync(
        CreateAdminUserRequest request,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        string role = NormalizeCreatableRole(
            request.Role);

        string email =
            request.Email.Trim().ToLowerInvariant();

        ApplicationUser? existingUser =
            await _dbContext.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    user =>
                        user.NormalizedEmail ==
                        email.ToUpperInvariant(),
                    cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            IsActive = true,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        IdentityResult createResult =
            await _userManager.CreateAsync(
                user,
                request.Password);

        EnsureIdentitySucceeded(
            createResult,
            "The user could not be created.");

        IdentityResult roleResult =
            await _userManager.AddToRoleAsync(
                user,
                role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            EnsureIdentitySucceeded(
                roleResult,
                "The user role could not be assigned.");
        }

        AddAudit(
            adminUserId,
            "UserCreated",
            "ApplicationUser",
            user.Id,
            $"Created {role} account {email}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapUser(
            user,
            [role]);
    }

    public async Task<AdminUserDto> UpdateUserAsync(
        string id,
        UpdateAdminUserRequest request,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        ApplicationUser user =
            await GetUserIncludingInactiveAsync(
                id,
                cancellationToken);

        string newEmail =
            request.Email.Trim().ToLowerInvariant();

        ApplicationUser? duplicateEmail =
            await _dbContext.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    candidate =>
                        candidate.Id != id &&
                        candidate.NormalizedEmail ==
                        newEmail.ToUpperInvariant(),
                    cancellationToken);

        if (duplicateEmail is not null)
        {
            throw new ConflictException(
                "Another user already uses this email address.");
        }

        user.FullName =
            request.FullName.Trim();

        user.Email = newEmail;
        user.UserName = newEmail;
        user.UpdatedAtUtc = DateTime.UtcNow;

        IdentityResult result =
            await _userManager.UpdateAsync(user);

        EnsureIdentitySucceeded(
            result,
            "The user could not be updated.");

        IReadOnlyCollection<string> roles =
            (await _userManager.GetRolesAsync(user))
            .ToArray();

        AddAudit(
            adminUserId,
            "UserUpdated",
            "ApplicationUser",
            user.Id,
            $"Updated account {newEmail}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapUser(user, roles);
    }

    public async Task SetUserStatusAsync(
        string id,
        bool isActive,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        ApplicationUser user =
            await GetUserIncludingInactiveAsync(
                id,
                cancellationToken);

        if (user.IsActive == isActive)
        {
            return;
        }

        bool isAdmin =
            await _userManager.IsInRoleAsync(
                user,
                AppRoles.Admin);

        if (!isActive && isAdmin)
        {
            int activeAdminCount =
                await CountActiveAdminsAsync(
                    cancellationToken);

            if (activeAdminCount <= 1)
            {
                throw new BusinessRuleException(
                    "The last active Admin account cannot be deactivated.");
            }
        }

        user.IsActive = isActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        IdentityResult updateResult =
            await _userManager.UpdateAsync(user);

        EnsureIdentitySucceeded(
            updateResult,
            "The user status could not be updated.");

        if (!isActive)
        {
            DateTime now = DateTime.UtcNow;

            List<RefreshToken> tokens =
                await _dbContext.RefreshTokens
                    .Where(token =>
                        token.UserId == user.Id &&
                        token.RevokedAtUtc == null)
                    .ToListAsync(
                        cancellationToken);

            foreach (RefreshToken token in tokens)
            {
                token.RevokedAtUtc = now;
                token.RevokedReason =
                    "User deactivated";
            }
        }

        AddAudit(
            adminUserId,
            isActive
                ? "UserActivated"
                : "UserDeactivated",
            "ApplicationUser",
            user.Id,
            isActive
                ? $"Activated account {user.Email}."
                : $"Deactivated account {user.Email}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task ResetPasswordAsync(
        string id,
        string newPassword,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        ApplicationUser user =
            await GetUserIncludingInactiveAsync(
                id,
                cancellationToken);

        string resetToken =
            await _userManager
                .GeneratePasswordResetTokenAsync(
                    user);

        IdentityResult resetResult =
            await _userManager.ResetPasswordAsync(
                user,
                resetToken,
                newPassword);

        EnsureIdentitySucceeded(
            resetResult,
            "The password could not be reset.");

        DateTime now = DateTime.UtcNow;

        List<RefreshToken> tokens =
            await _dbContext.RefreshTokens
                .Where(token =>
                    token.UserId == user.Id &&
                    token.RevokedAtUtc == null)
                .ToListAsync(
                    cancellationToken);

        foreach (RefreshToken token in tokens)
        {
            token.RevokedAtUtc = now;
            token.RevokedReason =
                "Password reset";
        }

        AddAudit(
            adminUserId,
            "PasswordReset",
            "ApplicationUser",
            user.Id,
            $"Reset password for account {user.Email}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<ApplicationUser>
        GetUserIncludingInactiveAsync(
            string id,
            CancellationToken cancellationToken)
    {
        ApplicationUser? user =
            await _dbContext.Users
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    candidate =>
                        candidate.Id == id,
                    cancellationToken);

        return user ??
            throw new ResourceNotFoundException(
                "The requested user was not found.");
    }

    private async Task<int> CountActiveAdminsAsync(
        CancellationToken cancellationToken)
    {
        string? adminRoleId =
            await _dbContext.Roles
                .Where(role =>
                    role.Name == AppRoles.Admin)
                .Select(role => role.Id)
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (adminRoleId is null)
        {
            return 0;
        }

        return await (
            from user in _dbContext.Users
                .IgnoreQueryFilters()
            join userRole in _dbContext.UserRoles
                on user.Id equals userRole.UserId
            where userRole.RoleId == adminRoleId &&
                  user.IsActive
            select user.Id)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    private void AddAudit(
        string userId,
        string action,
        string entityType,
        string entityId,
        string description)
    {
        _dbContext.AuditLogs.Add(
            new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                CreatedAtUtc = DateTime.UtcNow
            });
    }

    private static AdminUserDto MapUser(
        ApplicationUser user,
        IReadOnlyCollection<string> roles)
    {
        return new AdminUserDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            roles,
            user.IsActive,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }

    private static string NormalizeCreatableRole(
        string requestedRole)
    {
        if (requestedRole.Equals(
                AppRoles.Teacher,
                StringComparison.OrdinalIgnoreCase))
        {
            return AppRoles.Teacher;
        }

        if (requestedRole.Equals(
                AppRoles.Student,
                StringComparison.OrdinalIgnoreCase))
        {
            return AppRoles.Student;
        }

        throw new BusinessRuleException(
            "Admin-created accounts must use either the Teacher or Student role.");
    }

    private static void EnsureIdentitySucceeded(
        IdentityResult result,
        string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        string details = string.Join(
            "; ",
            result.Errors.Select(error =>
                error.Description));

        throw new BusinessRuleException(
            $"{message} {details}");
    }
}