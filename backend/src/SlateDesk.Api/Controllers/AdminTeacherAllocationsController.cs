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
[Route("api/v1/admin/teacher-allocations")]
public sealed class AdminTeacherAllocationsController
    : ControllerBase
{
    private readonly IAdminSetupService _service;

    public AdminTeacherAllocationsController(
        IAdminSetupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<
        PagedResult<TeacherAllocationDto>>> Get(
        [FromQuery] SetupListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetTeacherAllocationsAsync(
                    query,
                    cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<
        TeacherAllocationDto>> Create(
        CreateTeacherAllocationRequest request,
        CancellationToken cancellationToken)
    {
        TeacherAllocationDto result =
            await _service
                .CreateTeacherAllocationAsync(
                    request,
                    CurrentUserId(),
                    cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _service
            .RemoveTeacherAllocationAsync(
                id,
                CurrentUserId(),
                cancellationToken);

        return NoContent();
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(
                   ClaimTypes.NameIdentifier)
            ?? throw new AuthenticationFailedException();
    }
}