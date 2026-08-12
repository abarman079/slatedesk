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
[Route("api/v1/admin/subjects")]
public sealed class AdminSubjectsController
    : ControllerBase
{
    private readonly IAdminAcademicService _service;

    public AdminSubjectsController(
        IAdminAcademicService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<
        PagedResult<SubjectDto>>> GetSubjects(
        [FromQuery] AcademicListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetSubjectsAsync(
                query,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubjectDto>>
        GetSubject(
            Guid id,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetSubjectAsync(
                id,
                cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<SubjectDto>>
        CreateSubject(
            SaveSubjectRequest request,
            CancellationToken cancellationToken)
    {
        SubjectDto result =
            await _service.CreateSubjectAsync(
                request,
                CurrentUserId(),
                cancellationToken);

        return CreatedAtAction(
            nameof(GetSubject),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SubjectDto>>
        UpdateSubject(
            Guid id,
            SaveSubjectRequest request,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.UpdateSubjectAsync(
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
        await _service.SetSubjectStatusAsync(
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