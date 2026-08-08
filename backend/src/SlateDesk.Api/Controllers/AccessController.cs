using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SlateDesk.Domain.Constants;

namespace SlateDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/access")]
public sealed class AccessController : ControllerBase
{
    [HttpGet("admin")]
    [Authorize(Policy = AppPolicies.AdminOnly)]
    public IActionResult CheckAdminAccess()
    {
        return Ok(new
        {
            role = AppRoles.Admin,
            authorized = true,
            message = "Admin authorization succeeded."
        });
    }

    [HttpGet("teacher")]
    [Authorize(Policy = AppPolicies.TeacherOnly)]
    public IActionResult CheckTeacherAccess()
    {
        return Ok(new
        {
            role = AppRoles.Teacher,
            authorized = true,
            message = "Teacher authorization succeeded."
        });
    }

    [HttpGet("student")]
    [Authorize(Policy = AppPolicies.StudentOnly)]
    public IActionResult CheckStudentAccess()
    {
        return Ok(new
        {
            role = AppRoles.Student,
            authorized = true,
            message = "Student authorization succeeded."
        });
    }
}