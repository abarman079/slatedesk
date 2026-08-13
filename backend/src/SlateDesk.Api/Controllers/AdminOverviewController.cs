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
[Route("api/v1/admin")]
public sealed class AdminOverviewController
    : ControllerBase
{
    private readonly IAdminOverviewService
        _service;

    public AdminOverviewController(
        IAdminOverviewService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<
        AdminDashboardDto>> Dashboard(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetDashboardAsync(
                cancellationToken));
    }

    [HttpGet("assignments")]
    public async Task<ActionResult<
        PagedResult<
            AdminAssignmentOverviewDto>>>
        Assignments(
            [FromQuery]
            AdminAssignmentOverviewQuery query,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAssignmentsAsync(
                query,
                cancellationToken));
    }

    [HttpGet("submissions")]
    public async Task<ActionResult<
        PagedResult<
            AdminSubmissionOverviewDto>>>
        Submissions(
            [FromQuery]
            AdminSubmissionOverviewQuery query,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetSubmissionsAsync(
                query,
                cancellationToken));
    }

    [HttpGet("settings")]
    public async Task<ActionResult<
        IReadOnlyCollection<AppSettingDto>>>
        Settings(
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetSettingsAsync(
                cancellationToken));
    }

    [HttpPut("settings/{key}")]
    public async Task<ActionResult<AppSettingDto>>
        UpdateSetting(
            string key,
            UpdateAppSettingRequest request,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.UpdateSettingAsync(
                key,
                request,
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