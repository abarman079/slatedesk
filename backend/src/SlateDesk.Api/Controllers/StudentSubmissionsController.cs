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
[Authorize(Policy = AppPolicies.StudentOnly)]
[Route("api/v1/student")]
public sealed class StudentSubmissionsController
    : ControllerBase
{
    private readonly ISubmissionService _service;

    public StudentSubmissionsController(
        ISubmissionService service)
    {
        _service = service;
    }

    [HttpGet("submissions")]
    public async Task<ActionResult<
        PagedResult<StudentSubmissionDto>>> Get(
        [FromQuery] SubmissionListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetStudentSubmissionsAsync(
                    CurrentUserId(),
                    query,
                    cancellationToken));
    }

    [HttpGet("submissions/{id:guid}")]
    public async Task<
        ActionResult<StudentSubmissionDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetStudentSubmissionAsync(
                    id,
                    CurrentUserId(),
                    cancellationToken));
    }

    [HttpPost(
        "assignments/{assignmentId:guid}/submissions")]
    public async Task<
        ActionResult<StudentSubmissionDto>> SaveDraft(
        Guid assignmentId,
        SaveSubmissionDraftRequest request,
        CancellationToken cancellationToken)
    {
        StudentSubmissionDto result =
            await _service.SaveDraftAsync(
                assignmentId,
                CurrentUserId(),
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("submissions/{id:guid}")]
    public async Task<
        ActionResult<StudentSubmissionDto>> Update(
        Guid id,
        UpdateSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .UpdateStudentSubmissionAsync(
                    id,
                    CurrentUserId(),
                    request,
                    cancellationToken));
    }

    [HttpPost("submissions/{id:guid}/submit")]
    public async Task<
        ActionResult<StudentSubmissionDto>> Submit(
        Guid id,
        SubmitSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.SubmitAsync(
                id,
                CurrentUserId(),
                request,
                cancellationToken));
    }

    [HttpGet("results")]
    public async Task<ActionResult<
        PagedResult<StudentResultDto>>> Results(
        [FromQuery] SubmissionListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetStudentResultsAsync(
                CurrentUserId(),
                query,
                cancellationToken));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(
                   ClaimTypes.NameIdentifier)
            ?? throw new AuthenticationFailedException();
    }
}
