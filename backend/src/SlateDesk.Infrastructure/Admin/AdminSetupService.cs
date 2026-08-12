using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SlateDesk.Application.Admin.Interfaces;
using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Domain.Constants;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Identity;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.Admin;

public sealed class AdminSetupService
    : IAdminSetupService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminSetupService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<
        PagedResult<TeacherAllocationDto>>
        GetTeacherAllocationsAsync(
            SetupListQuery query,
            CancellationToken cancellationToken)
    {
        var data =
            from allocation in
                _dbContext.TeacherAllocations
                    .IgnoreQueryFilters()
                    .AsNoTracking()
            join teacher in
                _dbContext.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                on allocation.TeacherId
                equals teacher.Id
            join academicClass in
                _dbContext.AcademicClasses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                on allocation.AcademicClassId
                equals academicClass.Id
            join subject in
                _dbContext.Subjects
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                on allocation.SubjectId
                equals subject.Id
            select new
            {
                Allocation = allocation,
                Teacher = teacher,
                AcademicClass = academicClass,
                Subject = subject
            };

        if (query.IsActive.HasValue)
        {
            data = data.Where(item =>
                item.Allocation.IsActive ==
                query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            data = data.Where(item =>
                item.Teacher.FullName
                    .ToLower()
                    .Contains(search) ||
                item.AcademicClass.Name
                    .ToLower()
                    .Contains(search) ||
                item.AcademicClass.Code
                    .ToLower()
                    .Contains(search) ||
                item.Subject.Name
                    .ToLower()
                    .Contains(search) ||
                item.Subject.Code
                    .ToLower()
                    .Contains(search));
        }

        int count =
            await data.CountAsync(
                cancellationToken);

        TeacherAllocationDto[] items =
            await data
                .OrderBy(item =>
                    item.Teacher.FullName)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(item =>
                    new TeacherAllocationDto(
                        item.Allocation.Id,
                        item.Teacher.Id,
                        item.Teacher.FullName,
                        item.Teacher.Email ??
                            string.Empty,
                        item.AcademicClass.Id,
                        item.AcademicClass.Name,
                        item.AcademicClass.Code,
                        item.Subject.Id,
                        item.Subject.Name,
                        item.Subject.Code,
                        item.Allocation.IsActive,
                        item.Allocation
                            .AssignedAtUtc))
                .ToArrayAsync(
                    cancellationToken);

        return PagedResult<
            TeacherAllocationDto>.Create(
                items,
                query.Page,
                query.PageSize,
                count);
    }

    public async Task<TeacherAllocationDto>
        CreateTeacherAllocationAsync(
            CreateTeacherAllocationRequest request,
            string adminUserId,
            CancellationToken cancellationToken)
    {
        ApplicationUser teacher =
            await GetActiveUserAsync(
                request.TeacherId,
                cancellationToken);

        if (!await _userManager.IsInRoleAsync(
                teacher,
                AppRoles.Teacher))
        {
            throw new BusinessRuleException(
                "The selected user is not a Teacher.");
        }

        AcademicClass academicClass =
            await _dbContext.AcademicClasses
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        request.AcademicClassId,
                    cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The selected active class was not found.");

        Subject subject =
            await _dbContext.Subjects
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        request.SubjectId,
                    cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The selected active subject was not found.");

        TeacherAllocation? existing =
            await _dbContext.TeacherAllocations
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    item =>
                        item.TeacherId ==
                            teacher.Id &&
                        item.AcademicClassId ==
                            academicClass.Id &&
                        item.SubjectId ==
                            subject.Id,
                    cancellationToken);

        TeacherAllocation allocation;

        if (existing is not null)
        {
            if (existing.IsActive)
            {
                throw new ConflictException(
                    "This Teacher is already allocated to the selected class and subject.");
            }

            existing.IsActive = true;
            existing.AssignedAtUtc =
                DateTime.UtcNow;

            allocation = existing;
        }
        else
        {
            allocation =
                new TeacherAllocation
                {
                    TeacherId = teacher.Id,
                    AcademicClassId =
                        academicClass.Id,
                    SubjectId = subject.Id,
                    AssignedAtUtc =
                        DateTime.UtcNow,
                    IsActive = true
                };

            _dbContext.TeacherAllocations.Add(
                allocation);
        }

        AddAudit(
            adminUserId,
            "TeacherAllocated",
            "TeacherAllocation",
            allocation.Id.ToString(),
            $"Allocated {teacher.FullName} to {academicClass.Code} / {subject.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new TeacherAllocationDto(
            allocation.Id,
            teacher.Id,
            teacher.FullName,
            teacher.Email ?? string.Empty,
            academicClass.Id,
            academicClass.Name,
            academicClass.Code,
            subject.Id,
            subject.Name,
            subject.Code,
            allocation.IsActive,
            allocation.AssignedAtUtc);
    }

    public async Task RemoveTeacherAllocationAsync(
        Guid id,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        TeacherAllocation allocation =
            await _dbContext.TeacherAllocations
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(
                    item => item.Id == id,
                    cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The Teacher allocation was not found.");

        bool hasAssignments =
            await _dbContext.Assignments
                .AnyAsync(
                    assignment =>
                        assignment.TeacherId ==
                            allocation.TeacherId &&
                        assignment.AcademicClassId ==
                            allocation.AcademicClassId &&
                        assignment.SubjectId ==
                            allocation.SubjectId,
                    cancellationToken);

        if (hasAssignments)
        {
            allocation.IsActive = false;
        }
        else
        {
            _dbContext.TeacherAllocations.Remove(
                allocation);
        }

        AddAudit(
            adminUserId,
            hasAssignments
                ? "TeacherAllocationDeactivated"
                : "TeacherAllocationRemoved",
            "TeacherAllocation",
            allocation.Id.ToString(),
            hasAssignments
                ? "Teacher allocation was deactivated because related assignments exist."
                : "Teacher allocation was removed.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<
        PagedResult<StudentEnrollmentDto>>
        GetEnrollmentsAsync(
            SetupListQuery query,
            CancellationToken cancellationToken)
    {
        var data =
            from enrollment in
                _dbContext.StudentEnrollments
                    .IgnoreQueryFilters()
                    .AsNoTracking()
            join student in
                _dbContext.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                on enrollment.StudentId
                equals student.Id
            join academicClass in
                _dbContext.AcademicClasses
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                on enrollment.AcademicClassId
                equals academicClass.Id
            select new
            {
                Enrollment = enrollment,
                Student = student,
                AcademicClass = academicClass
            };

        if (query.IsActive.HasValue)
        {
            data = data.Where(item =>
                item.Enrollment.IsActive ==
                query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            data = data.Where(item =>
                item.Student.FullName
                    .ToLower()
                    .Contains(search) ||
                item.AcademicClass.Name
                    .ToLower()
                    .Contains(search) ||
                item.AcademicClass.Code
                    .ToLower()
                    .Contains(search));
        }

        int count =
            await data.CountAsync(
                cancellationToken);

        StudentEnrollmentDto[] items =
            await data
                .OrderBy(item =>
                    item.Student.FullName)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(item =>
                    new StudentEnrollmentDto(
                        item.Enrollment.Id,
                        item.Student.Id,
                        item.Student.FullName,
                        item.Student.Email ??
                            string.Empty,
                        item.AcademicClass.Id,
                        item.AcademicClass.Name,
                        item.AcademicClass.Code,
                        item.Enrollment.IsActive,
                        item.Enrollment
                            .EnrolledAtUtc))
                .ToArrayAsync(
                    cancellationToken);

        return PagedResult<
            StudentEnrollmentDto>.Create(
                items,
                query.Page,
                query.PageSize,
                count);
    }

    public async Task<StudentEnrollmentDto>
        CreateEnrollmentAsync(
            CreateStudentEnrollmentRequest request,
            string adminUserId,
            CancellationToken cancellationToken)
    {
        ApplicationUser student =
            await GetActiveUserAsync(
                request.StudentId,
                cancellationToken);

        if (!await _userManager.IsInRoleAsync(
                student,
                AppRoles.Student))
        {
            throw new BusinessRuleException(
                "The selected user is not a Student.");
        }

        AcademicClass academicClass =
            await _dbContext.AcademicClasses
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        request.AcademicClassId,
                    cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The selected active class was not found.");

        bool alreadyEnrolled =
            await _dbContext.StudentEnrollments
                .AnyAsync(
                    item =>
                        item.StudentId ==
                        student.Id,
                    cancellationToken);

        if (alreadyEnrolled)
        {
            throw new ConflictException(
                "This Student already has an active class enrollment.");
        }

        var enrollment =
            new StudentEnrollment
            {
                StudentId = student.Id,
                AcademicClassId =
                    academicClass.Id,
                EnrolledAtUtc =
                    DateTime.UtcNow,
                IsActive = true
            };

        _dbContext.StudentEnrollments.Add(
            enrollment);

        AddAudit(
            adminUserId,
            "StudentEnrolled",
            "StudentEnrollment",
            enrollment.Id.ToString(),
            $"Enrolled {student.FullName} in {academicClass.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CreateEnrollmentDto(
            enrollment,
            student,
            academicClass);
    }

    public async Task<StudentEnrollmentDto>
        UpdateEnrollmentAsync(
            Guid id,
            UpdateStudentEnrollmentRequest request,
            string adminUserId,
            CancellationToken cancellationToken)
    {
        StudentEnrollment enrollment =
            await GetEnrollmentEntityAsync(
                id,
                cancellationToken);

        AcademicClass academicClass =
            await _dbContext.AcademicClasses
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                        request.AcademicClassId,
                    cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The selected active class was not found.");

        enrollment.AcademicClassId =
            academicClass.Id;

        ApplicationUser student =
            await GetUserIncludingInactiveAsync(
                enrollment.StudentId,
                cancellationToken);

        AddAudit(
            adminUserId,
            "StudentEnrollmentUpdated",
            "StudentEnrollment",
            enrollment.Id.ToString(),
            $"Moved {student.FullName} to {academicClass.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return CreateEnrollmentDto(
            enrollment,
            student,
            academicClass);
    }

    public async Task SetEnrollmentStatusAsync(
        Guid id,
        bool isActive,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        StudentEnrollment enrollment =
            await GetEnrollmentEntityAsync(
                id,
                cancellationToken);

        if (isActive &&
            !enrollment.IsActive)
        {
            bool otherActiveEnrollment =
                await _dbContext.StudentEnrollments
                    .AnyAsync(
                        item =>
                            item.StudentId ==
                                enrollment.StudentId &&
                            item.Id != enrollment.Id,
                        cancellationToken);

            if (otherActiveEnrollment)
            {
                throw new ConflictException(
                    "This Student already has another active enrollment.");
            }

            ApplicationUser student =
                await GetActiveUserAsync(
                    enrollment.StudentId,
                    cancellationToken);

            if (!await _userManager.IsInRoleAsync(
                    student,
                    AppRoles.Student))
            {
                throw new BusinessRuleException(
                    "The selected user is not an active Student.");
            }

            bool classIsActive =
                await _dbContext.AcademicClasses
                    .AnyAsync(
                        item =>
                            item.Id ==
                            enrollment.AcademicClassId,
                        cancellationToken);

            if (!classIsActive)
            {
                throw new BusinessRuleException(
                    "The enrollment cannot be activated because its class is inactive.");
            }
        }

        enrollment.IsActive = isActive;

        AddAudit(
            adminUserId,
            isActive
                ? "StudentEnrollmentActivated"
                : "StudentEnrollmentDeactivated",
            "StudentEnrollment",
            enrollment.Id.ToString(),
            isActive
                ? "Student enrollment activated."
                : "Student enrollment deactivated.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>>
        GetAuditLogsAsync(
            SetupListQuery query,
            CancellationToken cancellationToken)
    {
        IQueryable<AuditLog> logs =
            _dbContext.AuditLogs
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            logs = logs.Where(log =>
                log.Action.ToLower()
                    .Contains(search) ||
                log.EntityType.ToLower()
                    .Contains(search) ||
                log.Description.ToLower()
                    .Contains(search));
        }

        int count =
            await logs.CountAsync(
                cancellationToken);

        AuditLogDto[] items =
            await logs
                .OrderByDescending(log =>
                    log.CreatedAtUtc)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(log =>
                    new AuditLogDto(
                        log.Id,
                        log.UserId,
                        log.Action,
                        log.EntityType,
                        log.EntityId,
                        log.Description,
                        log.CreatedAtUtc))
                .ToArrayAsync(
                    cancellationToken);

        return PagedResult<AuditLogDto>.Create(
            items,
            query.Page,
            query.PageSize,
            count);
    }

    private async Task<ApplicationUser>
        GetActiveUserAsync(
            string id,
            CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Id == id,
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The selected active user was not found.");
    }

    private async Task<ApplicationUser>
        GetUserIncludingInactiveAsync(
            string id,
            CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                user => user.Id == id,
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The selected user was not found.");
    }

    private async Task<StudentEnrollment>
        GetEnrollmentEntityAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        return await _dbContext.StudentEnrollments
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The Student enrollment was not found.");
    }

    private void AddAudit(
        string userId,
        string action,
        string entityType,
        string entityId,
        string description)
    {
        _dbContext.AuditLogs.Add(
            new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Description = description,
                CreatedAtUtc = DateTime.UtcNow
            });
    }

    private static StudentEnrollmentDto
        CreateEnrollmentDto(
            StudentEnrollment enrollment,
            ApplicationUser student,
            AcademicClass academicClass)
    {
        return new StudentEnrollmentDto(
            enrollment.Id,
            student.Id,
            student.FullName,
            student.Email ?? string.Empty,
            academicClass.Id,
            academicClass.Name,
            academicClass.Code,
            enrollment.IsActive,
            enrollment.EnrolledAtUtc);
    }
}