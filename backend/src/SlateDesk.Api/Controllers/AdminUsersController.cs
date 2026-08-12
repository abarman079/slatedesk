using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlateDesk.Application.Admin.Interfaces;
using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Domain.Constants;

namespace SlateDesk.Api.Controllers;

[ApiController]
[Authorize(Policy = AppPolicies.AdminOnly)]
[Route("api/v1/admin/users")]
public sealed class AdminUsersController
    : ControllerBase
{
    private readonly IAdminUserService _service;

    public AdminUsersController(
        IAdminUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<
        PagedResult<AdminUserDto>>> GetUsers(
        [FromQuery] AdminUserQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetUsersAsync(
                query,
                cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminUserDto>>
        GetUser(
            string id,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetUserAsync(
                id,
                cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserDto>>
        CreateUser(
            CreateAdminUserRequest request,
            CancellationToken cancellationToken)
    {
        AdminUserDto user =
            await _service.CreateUserAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

        return CreatedAtAction(
            nameof(GetUser),
            new { id = user.Id },
            user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminUserDto>>
        UpdateUser(
            string id,
            UpdateAdminUserRequest request,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.UpdateUserAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> SetStatus(
        string id,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _service.SetUserStatusAsync(
            id,
            request.IsActive,
            GetCurrentUserId(),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        string id,
        ResetUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _service.ResetPasswordAsync(
            id,
            request.NewPassword,
            GetCurrentUserId(),
            cancellationToken);

        return NoContent();
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(
                   ClaimTypes.NameIdentifier)
            ?? throw new AuthenticationFailedException();
    }
}