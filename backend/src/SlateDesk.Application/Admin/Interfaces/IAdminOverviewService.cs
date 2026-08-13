using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Models;

namespace SlateDesk.Application.Admin.Interfaces;

public interface IAdminOverviewService
{
    Task<AdminDashboardDto> GetDashboardAsync(
        CancellationToken cancellationToken);

    Task<PagedResult<AdminAssignmentOverviewDto>>
        GetAssignmentsAsync(
            AdminAssignmentOverviewQuery query,
            CancellationToken cancellationToken);

    Task<PagedResult<AdminSubmissionOverviewDto>>
        GetSubmissionsAsync(
            AdminSubmissionOverviewQuery query,
            CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AppSettingDto>>
        GetSettingsAsync(
            CancellationToken cancellationToken);

    Task<AppSettingDto> UpdateSettingAsync(
        string key,
        UpdateAppSettingRequest request,
        string adminUserId,
        CancellationToken cancellationToken);
}