using Microsoft.EntityFrameworkCore;
using SlateDesk.Application.Admin.Interfaces;
using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Domain.Constants;
using SlateDesk.Domain.Entities;
using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.Admin;

public sealed class AdminOverviewService
    : IAdminOverviewService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminOverviewService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardDto>
        GetDashboardAsync(
            CancellationToken cancellationToken)
    {
        var roleCounts =
            await (
                from user in _dbContext.Users.AsNoTracking()
                join userRole in _dbContext.UserRoles
                    on user.Id equals userRole.UserId
                join role in _dbContext.Roles
                    on userRole.RoleId equals role.Id
                where role.Name != null
                group user by role.Name! into grouped
                select new
                {
                    Role = grouped.Key,
                    Count = grouped.Count()
                })
                .ToListAsync(cancellationToken);

        int teachers =
            roleCounts
                .SingleOrDefault(item =>
                    item.Role == AppRoles.Teacher)
                ?.Count ?? 0;

        int students =
            roleCounts
                .SingleOrDefault(item =>
                    item.Role == AppRoles.Student)
                ?.Count ?? 0;

        int classes =
            await _dbContext.AcademicClasses
                .AsNoTracking()
                .CountAsync(cancellationToken);

        int subjects =
            await _dbContext.Subjects
                .AsNoTracking()
                .CountAsync(cancellationToken);

        int publishedAssignments =
            await _dbContext.Assignments
                .AsNoTracking()
                .CountAsync(
                    assignment =>
                        assignment.Status ==
                        AssignmentStatus.Published,
                    cancellationToken);

        int totalSubmissions =
            await _dbContext.Submissions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .CountAsync(cancellationToken);

        AuditLogDto[] recentActivity =
            await _dbContext.AuditLogs
                .AsNoTracking()
                .OrderByDescending(log =>
                    log.CreatedAtUtc)
                .Take(8)
                .Select(log =>
                    new AuditLogDto(
                        log.Id,
                        log.UserId,
                        log.Action,
                        log.EntityType,
                        log.EntityId,
                        log.Description,
                        log.CreatedAtUtc))
                .ToArrayAsync(cancellationToken);

        return new AdminDashboardDto(
            teachers,
            students,
            classes,
            subjects,
            publishedAssignments,
            totalSubmissions,
            recentActivity);
    }

    public async Task<
        PagedResult<AdminAssignmentOverviewDto>>
        GetAssignmentsAsync(
            AdminAssignmentOverviewQuery query,
            CancellationToken cancellationToken)
    {
        IQueryable<Assignment> assignments =
            _dbContext.Assignments
                .IgnoreQueryFilters()
                .AsNoTracking();

        if (query.Status.HasValue)
        {
            assignments = assignments.Where(
                assignment =>
                    assignment.Status ==
                    query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            assignments = assignments.Where(
                assignment =>
                    assignment.Title
                        .ToLower()
                        .Contains(search) ||
                    assignment.AcademicClass.Name
                        .ToLower()
                        .Contains(search) ||
                    assignment.AcademicClass.Code
                        .ToLower()
                        .Contains(search) ||
                    assignment.Subject.Name
                        .ToLower()
                        .Contains(search) ||
                    assignment.Subject.Code
                        .ToLower()
                        .Contains(search));
        }

        int totalItems =
            await assignments.CountAsync(
                cancellationToken);

        AdminAssignmentOverviewDto[] items =
            await assignments
                .OrderByDescending(
                    assignment =>
                        assignment.CreatedAtUtc)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(assignment =>
                    new AdminAssignmentOverviewDto(
                        assignment.Id,
                        assignment.Title,
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                assignment.TeacherId)
                            .Select(user =>
                                user.FullName)
                            .FirstOrDefault()
                            ?? "Teacher",
                        assignment.AcademicClass.Name,
                        assignment.AcademicClass.Code,
                        assignment.Subject.Name,
                        assignment.Subject.Code,
                        assignment.DeadlineUtc,
                        assignment.MaximumMarks,
                        assignment.Status,
                        _dbContext.Submissions
                            .IgnoreQueryFilters()
                            .Count(submission =>
                                submission.AssignmentId ==
                                assignment.Id),
                        assignment.IsArchived))
                .ToArrayAsync(cancellationToken);

        return PagedResult<
            AdminAssignmentOverviewDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems);
    }

    public async Task<
        PagedResult<AdminSubmissionOverviewDto>>
        GetSubmissionsAsync(
            AdminSubmissionOverviewQuery query,
            CancellationToken cancellationToken)
    {
        IQueryable<Submission> submissions =
            _dbContext.Submissions
                .IgnoreQueryFilters()
                .AsNoTracking();

        if (query.Status.HasValue)
        {
            submissions = submissions.Where(
                submission =>
                    submission.Status ==
                    query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            submissions = submissions.Where(
                submission =>
                    submission.Assignment.Title
                        .ToLower()
                        .Contains(search) ||
                    _dbContext.Users
                        .IgnoreQueryFilters()
                        .Any(user =>
                            user.Id ==
                                submission.StudentId &&
                            user.FullName
                                .ToLower()
                                .Contains(search)));
        }

        int totalItems =
            await submissions.CountAsync(
                cancellationToken);

        AdminSubmissionOverviewDto[] items =
            await submissions
                .OrderByDescending(
                    submission =>
                        submission.UpdatedAtUtc)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(submission =>
                    new AdminSubmissionOverviewDto(
                        submission.Id,
                        submission.AssignmentId,
                        submission.Assignment.Title,
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                submission.StudentId)
                            .Select(user =>
                                user.FullName)
                            .FirstOrDefault()
                            ?? "Student",
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                submission.Assignment
                                    .TeacherId)
                            .Select(user =>
                                user.FullName)
                            .FirstOrDefault()
                            ?? "Teacher",
                        submission.SubmittedAtUtc,
                        submission.Status,
                        submission.MarksAwarded,
                        submission.Assignment
                            .MaximumMarks,
                        submission.GradedAtUtc))
                .ToArrayAsync(cancellationToken);

        return PagedResult<
            AdminSubmissionOverviewDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems);
    }

    public async Task<
        IReadOnlyCollection<AppSettingDto>>
        GetSettingsAsync(
            CancellationToken cancellationToken)
    {
        return await _dbContext.AppSettings
            .AsNoTracking()
            .OrderBy(setting =>
                setting.Key)
            .Select(setting =>
                new AppSettingDto(
                    setting.Id,
                    setting.Key,
                    setting.Value,
                    setting.Description,
                    setting.UpdatedAtUtc))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<AppSettingDto>
        UpdateSettingAsync(
            string key,
            UpdateAppSettingRequest request,
            string adminUserId,
            CancellationToken cancellationToken)
    {
        AppSetting setting =
            await _dbContext.AppSettings
                .SingleOrDefaultAsync(
                    item => item.Key == key,
                    cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The application setting was not found.");

        string value =
            request.Value.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleException(
                "Setting value cannot be empty.");
        }

        setting.Value = value;
        setting.UpdatedAtUtc =
            DateTime.UtcNow;
        setting.UpdatedByUserId =
            adminUserId;

        _dbContext.AuditLogs.Add(
            new AuditLog
            {
                UserId = adminUserId,
                Action = "AppSettingUpdated",
                EntityType = "AppSetting",
                EntityId =
                    setting.Id.ToString(),
                Description =
                    $"Updated application setting '{setting.Key}'.",
                CreatedAtUtc =
                    DateTime.UtcNow
            });

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new AppSettingDto(
            setting.Id,
            setting.Key,
            setting.Value,
            setting.Description,
            setting.UpdatedAtUtc);
    }
}