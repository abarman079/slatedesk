using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Application.Submissions.Interfaces;
using SlateDesk.Application.Submissions.Models;
using SlateDesk.Domain.Constants;

namespace SlateDesk.Api.Controllers;

[ApiController]
[Authorize(Policy = AppPolicies.TeacherOnly)]
[Route("api/v1/teacher")]
public sealed class TeacherSubmissionsController
    : ControllerBase
{
    private readonly ISubmissionService _service;

    public TeacherSubmissionsController(
        ISubmissionService service)
    {
        _service = service;
    }

    [HttpGet(
        "assignments/{assignmentId:guid}/submissions")]
    public async Task<ActionResult<
        PagedResult<TeacherSubmissionDto>>>
        GetAssignmentSubmissions(
            Guid assignmentId,
            [FromQuery] SubmissionListQuery query,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetTeacherAssignmentSubmissionsAsync(
                    assignmentId,
                    CurrentUserId(),
                    query,
                    cancellationToken));
    }

    [HttpGet("submissions/{id:guid}")]
    public async Task<
        ActionResult<TeacherSubmissionDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetTeacherSubmissionAsync(
                    id,
                    CurrentUserId(),
                    cancellationToken));
    }

    [HttpPut(
        "submissions/{id:guid}/review-status")]
    public async Task<
        ActionResult<TeacherSubmissionDto>>
        UpdateReviewStatus(
            Guid id,
            UpdateReviewStatusRequest request,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .UpdateReviewStatusAsync(
                    id,
                    CurrentUserId(),
                    request,
                    cancellationToken));
    }

    [HttpPut("submissions/{id:guid}/grade")]
    public async Task<
        ActionResult<TeacherSubmissionDto>> Grade(
        Guid id,
        GradeSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GradeAsync(
                id,
                CurrentUserId(),
                request,
                cancellationToken));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(
                   ClaimTypes.NameIdentifier)
            ?? throw new AuthenticationFailedException();
    }
}
