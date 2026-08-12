using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlateDesk.Application.Assignments.Interfaces;
using SlateDesk.Application.Assignments.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Domain.Constants;

namespace SlateDesk.Api.Controllers;

[ApiController]
[Authorize(Policy = AppPolicies.TeacherOnly)]
[Route("api/v1/teacher/assignments")]
public sealed class TeacherAssignmentsController
    : ControllerBase
{
    private readonly IAssignmentService _service;

    public TeacherAssignmentsController(
        IAssignmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<
        PagedResult<TeacherAssignmentDto>>> Get(
        [FromQuery]
        TeacherAssignmentListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetTeacherAssignmentsAsync(
                    CurrentUserId(),
                    query,
                    cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<
        ActionResult<TeacherAssignmentDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetTeacherAssignmentAsync(
                    id,
                    CurrentUserId(),
                    cancellationToken));
    }

    [HttpPost]
    public async Task<
        ActionResult<TeacherAssignmentDto>> Create(
        CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        TeacherAssignmentDto result =
            await _service
                .CreateAssignmentAsync(
                    CurrentUserId(),
                    request,
                    cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:guid}")]
    public async Task<
        ActionResult<TeacherAssignmentDto>> Update(
        Guid id,
        UpdateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.UpdateAssignmentAsync(
                id,
                CurrentUserId(),
                request,
                cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAssignmentAsync(
            id,
            CurrentUserId(),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<
        ActionResult<TeacherAssignmentDto>> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .PublishAssignmentAsync(
                    id,
                    CurrentUserId(),
                    cancellationToken));
    }

    [HttpPost("{id:guid}/close")]
    public async Task<
        ActionResult<TeacherAssignmentDto>> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.CloseAssignmentAsync(
                id,
                CurrentUserId(),
                cancellationToken));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(
                   ClaimTypes.NameIdentifier)
            ?? throw new AuthenticationFailedException();
    }
}