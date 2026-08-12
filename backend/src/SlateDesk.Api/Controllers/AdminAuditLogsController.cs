using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlateDesk.Application.Admin.Interfaces;
using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Models;
using SlateDesk.Domain.Constants;

namespace SlateDesk.Api.Controllers;

[ApiController]
[Authorize(Policy = AppPolicies.AdminOnly)]
[Route("api/v1/admin/audit-logs")]
public sealed class AdminAuditLogsController
    : ControllerBase
{
    private readonly IAdminSetupService _service;

    public AdminAuditLogsController(
        IAdminSetupService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<
        PagedResult<AuditLogDto>>> Get(
        [FromQuery] SetupListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAuditLogsAsync(
                query,
                cancellationToken));
    }
}