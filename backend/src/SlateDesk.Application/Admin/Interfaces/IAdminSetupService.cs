using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Models;

namespace SlateDesk.Application.Admin.Interfaces;

public interface IAdminSetupService
{
    Task<PagedResult<TeacherAllocationDto>>
        GetTeacherAllocationsAsync(
            SetupListQuery query,
            CancellationToken cancellationToken);

    Task<TeacherAllocationDto>
        CreateTeacherAllocationAsync(
            CreateTeacherAllocationRequest request,
            string adminUserId,
            CancellationToken cancellationToken);

    Task RemoveTeacherAllocationAsync(
        Guid id,
        string adminUserId,
        CancellationToken cancellationToken);

    Task<PagedResult<StudentEnrollmentDto>>
        GetEnrollmentsAsync(
            SetupListQuery query,
            CancellationToken cancellationToken);

    Task<StudentEnrollmentDto>
        CreateEnrollmentAsync(
            CreateStudentEnrollmentRequest request,
            string adminUserId,
            CancellationToken cancellationToken);

    Task<StudentEnrollmentDto>
        UpdateEnrollmentAsync(
            Guid id,
            UpdateStudentEnrollmentRequest request,
            string adminUserId,
            CancellationToken cancellationToken);

    Task SetEnrollmentStatusAsync(
        Guid id,
        bool isActive,
        string adminUserId,
        CancellationToken cancellationToken);

    Task<PagedResult<AuditLogDto>>
        GetAuditLogsAsync(
            SetupListQuery query,
            CancellationToken cancellationToken);
}