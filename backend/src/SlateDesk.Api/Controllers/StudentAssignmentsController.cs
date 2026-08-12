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
[Authorize(Policy = AppPolicies.StudentOnly)]
[Route("api/v1/student/assignments")]
public sealed class StudentAssignmentsController
    : ControllerBase
{
    private readonly IAssignmentService _service;

    public StudentAssignmentsController(
        IAssignmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<
        PagedResult<StudentAssignmentDto>>> Get(
        [FromQuery]
        StudentAssignmentListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetStudentAssignmentsAsync(
                    CurrentUserId(),
                    query,
                    cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<
        ActionResult<StudentAssignmentDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service
                .GetStudentAssignmentAsync(
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