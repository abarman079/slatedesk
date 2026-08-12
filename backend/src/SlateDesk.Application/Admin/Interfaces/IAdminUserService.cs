using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Models;

namespace SlateDesk.Application.Admin.Interfaces;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserDto>> GetUsersAsync(
        AdminUserQuery query,
        CancellationToken cancellationToken);

    Task<AdminUserDto> GetUserAsync(
        string id,
        CancellationToken cancellationToken);

    Task<AdminUserDto> CreateUserAsync(
        CreateAdminUserRequest request,
        string adminUserId,
        CancellationToken cancellationToken);

    Task<AdminUserDto> UpdateUserAsync(
        string id,
        UpdateAdminUserRequest request,
        string adminUserId,
        CancellationToken cancellationToken);

    Task SetUserStatusAsync(
        string id,
        bool isActive,
        string adminUserId,
        CancellationToken cancellationToken);

    Task ResetPasswordAsync(
        string id,
        string newPassword,
        string adminUserId,
        CancellationToken cancellationToken);
}