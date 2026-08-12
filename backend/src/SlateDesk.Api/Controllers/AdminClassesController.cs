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
[Route("api/v1/admin/classes")]
public sealed class AdminClassesController
    : ControllerBase
{
    private readonly IAdminAcademicService _service;

    public AdminClassesController(
        IAdminAcademicService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<
        PagedResult<AcademicClassDto>>> GetClasses(
        [FromQuery] AcademicListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetClassesAsync(
                query,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AcademicClassDto>>
        GetClass(
            Guid id,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetClassAsync(
                id,
                cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AcademicClassDto>>
        CreateClass(
            SaveAcademicClassRequest request,
            CancellationToken cancellationToken)
    {
        AcademicClassDto result =
            await _service.CreateClassAsync(
                request,
                CurrentUserId(),
                cancellationToken);

        return CreatedAtAction(
            nameof(GetClass),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AcademicClassDto>>
        UpdateClass(
            Guid id,
            SaveAcademicClassRequest request,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.UpdateClassAsync(
                id,
                request,
                CurrentUserId(),
                cancellationToken));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(
        Guid id,
        UpdateResourceStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _service.SetClassStatusAsync(
            id,
            request.IsActive,
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