using Microsoft.EntityFrameworkCore;
using SlateDesk.Application.Assignments.Interfaces;
using SlateDesk.Application.Assignments.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Domain.Entities;
using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.Assignments;

public sealed class AssignmentService
    : IAssignmentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IAssignmentDeadlinePolicy
        _deadlinePolicy;

    public AssignmentService(
        ApplicationDbContext dbContext,
        IAssignmentDeadlinePolicy deadlinePolicy)
    {
        _dbContext = dbContext;
        _deadlinePolicy = deadlinePolicy;
    }

    public async Task<PagedResult<TeacherAssignmentDto>>
        GetTeacherAssignmentsAsync(
            string teacherId,
            TeacherAssignmentListQuery query,
            CancellationToken cancellationToken)
    {
        DateTime utcNow = DateTime.UtcNow;

        IQueryable<Assignment> assignments =
            _dbContext.Assignments
                .AsNoTracking()
                .Where(assignment =>
                    assignment.TeacherId ==
                    teacherId);

        if (query.Status.HasValue)
        {
            assignments = assignments.Where(
                assignment =>
                    assignment.Status ==
                    query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(
                query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            assignments = assignments.Where(
                assignment =>
                    assignment.Title
                        .ToLower()
                        .Contains(search) ||
                    assignment.Subject.Name
                        .ToLower()
                        .Contains(search) ||
                    assignment.Subject.Code
                        .ToLower()
                        .Contains(search) ||
                    assignment.AcademicClass.Name
                        .ToLower()
                        .Contains(search) ||
                    assignment.AcademicClass.Code
                        .ToLower()
                        .Contains(search));
        }

        int totalItems =
            await assignments.CountAsync(
                cancellationToken);

        TeacherAssignmentDto[] items =
            await assignments
                .OrderByDescending(
                    assignment =>
                        assignment.CreatedAtUtc)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(assignment =>
                    new TeacherAssignmentDto(
                        assignment.Id,
                        assignment.AcademicClassId,
                        assignment.AcademicClass.Name,
                        assignment.AcademicClass.Code,
                        assignment.SubjectId,
                        assignment.Subject.Name,
                        assignment.Subject.Code,
                        assignment.Title,
                        assignment.Description,
                        assignment.Instructions,
                        assignment.DeadlineUtc,
                        assignment.MaximumMarks,
                        assignment.AllowResubmission,
                        assignment.AllowLateSubmission,
                        assignment.Status,
                        assignment.PublishedAtUtc,
                        assignment.CreatedAtUtc,
                        assignment.UpdatedAtUtc,
                        assignment.Submissions.Count,
                        assignment.DeadlineUtc <= utcNow))
                .ToArrayAsync(
                    cancellationToken);

        return PagedResult<
            TeacherAssignmentDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems);
    }

    public async Task<TeacherAssignmentDto>
        GetTeacherAssignmentAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken)
    {
        DateTime utcNow = DateTime.UtcNow;

        return await _dbContext.Assignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.Id == id &&
                assignment.TeacherId ==
                teacherId)
            .Select(assignment =>
                new TeacherAssignmentDto(
                    assignment.Id,
                    assignment.AcademicClassId,
                    assignment.AcademicClass.Name,
                    assignment.AcademicClass.Code,
                    assignment.SubjectId,
                    assignment.Subject.Name,
                    assignment.Subject.Code,
                    assignment.Title,
                    assignment.Description,
                    assignment.Instructions,
                    assignment.DeadlineUtc,
                    assignment.MaximumMarks,
                    assignment.AllowResubmission,
                    assignment.AllowLateSubmission,
                    assignment.Status,
                    assignment.PublishedAtUtc,
                    assignment.CreatedAtUtc,
                    assignment.UpdatedAtUtc,
                    assignment.Submissions.Count,
                    assignment.DeadlineUtc <= utcNow))
            .SingleOrDefaultAsync(
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The assignment was not found.");
    }

    public async Task<
        IReadOnlyCollection<TeacherAllocationOptionDto>>
        GetTeacherAllocationOptionsAsync(
            string teacherId,
            CancellationToken cancellationToken)
    {
        return await _dbContext
            .TeacherAllocations
            .AsNoTracking()
            .Where(allocation =>
                allocation.TeacherId ==
                teacherId)
            .OrderBy(allocation =>
                allocation.AcademicClass.Code)
            .ThenBy(allocation =>
                allocation.Subject.Code)
            .Select(allocation =>
                new TeacherAllocationOptionDto(
                    allocation.AcademicClassId,
                    allocation.AcademicClass.Name,
                    allocation.AcademicClass.Code,
                    allocation.SubjectId,
                    allocation.Subject.Name,
                    allocation.Subject.Code))
            .ToArrayAsync(
                cancellationToken);
    }

    public async Task<TeacherAssignmentDto>
        CreateAssignmentAsync(
            string teacherId,
            CreateAssignmentRequest request,
            CancellationToken cancellationToken)
    {
        ValidateRequest(
            request.AcademicClassId,
            request.SubjectId,
            request.Title,
            request.Description,
            request.DeadlineUtc,
            request.MaximumMarks);

        await EnsureTeacherAllocationAsync(
            teacherId,
            request.AcademicClassId,
            request.SubjectId,
            cancellationToken);

        var assignment = new Assignment
        {
            TeacherId = teacherId,
            AcademicClassId =
                request.AcademicClassId,
            SubjectId = request.SubjectId,
            Title = request.Title.Trim(),
            Description =
                request.Description.Trim(),
            Instructions =
                NormalizeOptional(
                    request.Instructions),
            DeadlineUtc =
                NormalizeUtc(
                    request.DeadlineUtc),
            MaximumMarks =
                request.MaximumMarks,
            AllowResubmission =
                request.AllowResubmission,
            AllowLateSubmission =
                request.AllowLateSubmission,
            Status = AssignmentStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow,
            IsArchived = false
        };

        _dbContext.Assignments.Add(assignment);

        AddAudit(
            teacherId,
            "AssignmentCreated",
            assignment.Id,
            $"Created draft assignment '{assignment.Title}'.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetTeacherAssignmentAsync(
            assignment.Id,
            teacherId,
            cancellationToken);
    }

    public async Task<TeacherAssignmentDto>
        UpdateAssignmentAsync(
            Guid id,
            string teacherId,
            UpdateAssignmentRequest request,
            CancellationToken cancellationToken)
    {
        ValidateRequest(
            request.AcademicClassId,
            request.SubjectId,
            request.Title,
            request.Description,
            request.DeadlineUtc,
            request.MaximumMarks);

        Assignment assignment =
            await GetOwnedAssignmentEntityAsync(
                id,
                teacherId,
                cancellationToken);

        if (assignment.Status ==
            AssignmentStatus.Closed)
        {
            throw new BusinessRuleException(
                "A closed assignment cannot be edited.");
        }

        bool academicContextChanged =
            assignment.AcademicClassId !=
                request.AcademicClassId ||
            assignment.SubjectId !=
                request.SubjectId;

        if (academicContextChanged)
        {
            bool hasSubmissions =
                await _dbContext.Submissions
                    .AnyAsync(
                        submission =>
                            submission.AssignmentId ==
                            assignment.Id,
                        cancellationToken);

            if (hasSubmissions)
            {
                throw new ConflictException(
                    "Class or subject cannot be changed after submissions exist.");
            }
        }

        await EnsureTeacherAllocationAsync(
            teacherId,
            request.AcademicClassId,
            request.SubjectId,
            cancellationToken);

        DateTime deadlineUtc =
            NormalizeUtc(request.DeadlineUtc);

        if (assignment.Status ==
                AssignmentStatus.Published &&
            deadlineUtc <= DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "A published assignment must have a future deadline.");
        }

        assignment.AcademicClassId =
            request.AcademicClassId;

        assignment.SubjectId =
            request.SubjectId;

        assignment.Title =
            request.Title.Trim();

        assignment.Description =
            request.Description.Trim();

        assignment.Instructions =
            NormalizeOptional(
                request.Instructions);

        assignment.DeadlineUtc =
            deadlineUtc;

        assignment.MaximumMarks =
            request.MaximumMarks;

        assignment.AllowResubmission =
            request.AllowResubmission;

        assignment.AllowLateSubmission =
            request.AllowLateSubmission;

        assignment.UpdatedAtUtc =
            DateTime.UtcNow;

        AddAudit(
            teacherId,
            "AssignmentUpdated",
            assignment.Id,
            $"Updated assignment '{assignment.Title}'.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetTeacherAssignmentAsync(
            assignment.Id,
            teacherId,
            cancellationToken);
    }

    public async Task DeleteAssignmentAsync(
        Guid id,
        string teacherId,
        CancellationToken cancellationToken)
    {
        Assignment assignment =
            await GetOwnedAssignmentEntityAsync(
                id,
                teacherId,
                cancellationToken);

        bool hasSubmissions =
            await _dbContext.Submissions
                .AnyAsync(
                    submission =>
                        submission.AssignmentId ==
                        assignment.Id,
                    cancellationToken);

        if (hasSubmissions)
        {
            assignment.IsArchived = true;
            assignment.Status =
                AssignmentStatus.Archived;
            assignment.UpdatedAtUtc =
                DateTime.UtcNow;

            AddAudit(
                teacherId,
                "AssignmentArchived",
                assignment.Id,
                $"Archived assignment '{assignment.Title}'.");
        }
        else
        {
            _dbContext.Assignments.Remove(
                assignment);

            AddAudit(
                teacherId,
                "AssignmentDeleted",
                assignment.Id,
                $"Deleted assignment '{assignment.Title}'.");
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<TeacherAssignmentDto>
        PublishAssignmentAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken)
    {
        Assignment assignment =
            await GetOwnedAssignmentEntityAsync(
                id,
                teacherId,
                cancellationToken);

        if (assignment.Status !=
            AssignmentStatus.Draft)
        {
            throw new BusinessRuleException(
                "Only a draft assignment can be published.");
        }

        if (string.IsNullOrWhiteSpace(
                assignment.Title) ||
            string.IsNullOrWhiteSpace(
                assignment.Description))
        {
            throw new BusinessRuleException(
                "Title and description are required before publication.");
        }

        if (assignment.MaximumMarks <= 0)
        {
            throw new BusinessRuleException(
                "Maximum marks must be greater than zero.");
        }

        if (assignment.DeadlineUtc <=
            DateTime.UtcNow)
        {
            throw new BusinessRuleException(
                "The assignment deadline must be in the future before publication.");
        }

        await EnsureTeacherAllocationAsync(
            teacherId,
            assignment.AcademicClassId,
            assignment.SubjectId,
            cancellationToken);

        assignment.Status =
            AssignmentStatus.Published;

        assignment.PublishedAtUtc =
            DateTime.UtcNow;

        assignment.UpdatedAtUtc =
            DateTime.UtcNow;

        AddAudit(
            teacherId,
            "AssignmentPublished",
            assignment.Id,
            $"Published assignment '{assignment.Title}'.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetTeacherAssignmentAsync(
            assignment.Id,
            teacherId,
            cancellationToken);
    }

    public async Task<TeacherAssignmentDto>
        CloseAssignmentAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken)
    {
        Assignment assignment =
            await GetOwnedAssignmentEntityAsync(
                id,
                teacherId,
                cancellationToken);

        if (assignment.Status ==
            AssignmentStatus.Closed)
        {
            return await GetTeacherAssignmentAsync(
                assignment.Id,
                teacherId,
                cancellationToken);
        }

        if (assignment.Status !=
            AssignmentStatus.Published)
        {
            throw new BusinessRuleException(
                "Only a published assignment can be closed.");
        }

        assignment.Status =
            AssignmentStatus.Closed;

        assignment.UpdatedAtUtc =
            DateTime.UtcNow;

        AddAudit(
            teacherId,
            "AssignmentClosed",
            assignment.Id,
            $"Closed assignment '{assignment.Title}'.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetTeacherAssignmentAsync(
            assignment.Id,
            teacherId,
            cancellationToken);
    }

    public async Task<
        PagedResult<StudentAssignmentDto>>
        GetStudentAssignmentsAsync(
            string studentId,
            StudentAssignmentListQuery query,
            CancellationToken cancellationToken)
    {
        Guid? classId =
            await GetActiveStudentClassIdAsync(
                studentId,
                cancellationToken);

        if (!classId.HasValue)
        {
            return PagedResult<
                StudentAssignmentDto>.Create(
                    [],
                    query.Page,
                    query.PageSize,
                    0);
        }

        IQueryable<Assignment> assignments =
            CreateStudentVisibleQuery(
                studentId,
                classId.Value);

        if (!string.IsNullOrWhiteSpace(
                query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            assignments = assignments.Where(
                assignment =>
                    assignment.Title
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

        StudentAssignmentRow[] rows =
            await assignments
                .OrderBy(
                    assignment =>
                        assignment.DeadlineUtc)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(assignment =>
                    new StudentAssignmentRow(
                        assignment.Id,
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                assignment.TeacherId)
                            .Select(user =>
                                user.FullName)
                            .FirstOrDefault() ??
                            "Teacher",
                        assignment.AcademicClassId,
                        assignment.AcademicClass.Name,
                        assignment.AcademicClass.Code,
                        assignment.SubjectId,
                        assignment.Subject.Name,
                        assignment.Subject.Code,
                        assignment.Title,
                        assignment.Description,
                        assignment.Instructions,
                        assignment.DeadlineUtc,
                        assignment.MaximumMarks,
                        assignment.AllowResubmission,
                        assignment.AllowLateSubmission,
                        assignment.Status,
                        _dbContext.Submissions
                            .Where(submission =>
                                submission.AssignmentId ==
                                    assignment.Id &&
                                submission.StudentId ==
                                    studentId)
                            .Select(submission =>
                                (SubmissionStatus?)
                                submission.Status)
                            .FirstOrDefault()))
                .ToArrayAsync(
                    cancellationToken);

        StudentAssignmentDto[] items =
            rows
                .Select(MapStudentAssignment)
                .ToArray();

        return PagedResult<
            StudentAssignmentDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems);
    }

    public async Task<StudentAssignmentDto>
        GetStudentAssignmentAsync(
            Guid id,
            string studentId,
            CancellationToken cancellationToken)
    {
        Guid? classId =
            await GetActiveStudentClassIdAsync(
                studentId,
                cancellationToken);

        if (!classId.HasValue)
        {
            throw new ResourceNotFoundException(
                "The assignment was not found.");
        }

        StudentAssignmentRow? row =
            await CreateStudentVisibleQuery(
                    studentId,
                    classId.Value)
                .Where(assignment =>
                    assignment.Id == id)
                .Select(assignment =>
                    new StudentAssignmentRow(
                        assignment.Id,
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                assignment.TeacherId)
                            .Select(user =>
                                user.FullName)
                            .FirstOrDefault() ??
                            "Teacher",
                        assignment.AcademicClassId,
                        assignment.AcademicClass.Name,
                        assignment.AcademicClass.Code,
                        assignment.SubjectId,
                        assignment.Subject.Name,
                        assignment.Subject.Code,
                        assignment.Title,
                        assignment.Description,
                        assignment.Instructions,
                        assignment.DeadlineUtc,
                        assignment.MaximumMarks,
                        assignment.AllowResubmission,
                        assignment.AllowLateSubmission,
                        assignment.Status,
                        _dbContext.Submissions
                            .Where(submission =>
                                submission.AssignmentId ==
                                    assignment.Id &&
                                submission.StudentId ==
                                    studentId)
                            .Select(submission =>
                                (SubmissionStatus?)
                                submission.Status)
                            .FirstOrDefault()))
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (row is null)
        {
            throw new ResourceNotFoundException(
                "The assignment was not found.");
        }

        return MapStudentAssignment(row);
    }

    private IQueryable<Assignment>
        CreateStudentVisibleQuery(
            string studentId,
            Guid classId)
    {
        return _dbContext.Assignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.AcademicClassId ==
                    classId &&
                (
                    assignment.Status ==
                        AssignmentStatus.Published ||
                    (
                        assignment.Status ==
                            AssignmentStatus.Closed &&
                        (
                            assignment.AllowLateSubmission ||
                            assignment.Submissions.Any(
                                submission =>
                                    submission.StudentId ==
                                    studentId)
                        )
                    )
                ));
    }

    private async Task<Guid?>
        GetActiveStudentClassIdAsync(
            string studentId,
            CancellationToken cancellationToken)
    {
        return await _dbContext
            .StudentEnrollments
            .AsNoTracking()
            .Where(enrollment =>
                enrollment.StudentId ==
                studentId)
            .Select(enrollment =>
                (Guid?)enrollment
                    .AcademicClassId)
            .SingleOrDefaultAsync(
                cancellationToken);
    }

    private async Task<Assignment>
        GetOwnedAssignmentEntityAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken)
    {
        return await _dbContext.Assignments
            .SingleOrDefaultAsync(
                assignment =>
                    assignment.Id == id &&
                    assignment.TeacherId ==
                    teacherId,
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The assignment was not found.");
    }

    private async Task
        EnsureTeacherAllocationAsync(
            string teacherId,
            Guid academicClassId,
            Guid subjectId,
            CancellationToken cancellationToken)
    {
        if (academicClassId == Guid.Empty ||
            subjectId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "A valid class and subject are required.");
        }

        bool valid =
            await _dbContext
                .TeacherAllocations
                .AnyAsync(
                    allocation =>
                        allocation.TeacherId ==
                            teacherId &&
                        allocation.AcademicClassId ==
                            academicClassId &&
                        allocation.SubjectId ==
                            subjectId &&
                        allocation.AcademicClass
                            .IsActive &&
                        allocation.Subject
                            .IsActive,
                    cancellationToken);

        if (!valid)
        {
            throw new UnauthorizedAccessException(
                "You are not allocated to this active class and subject.");
        }
    }

    private StudentAssignmentDto
        MapStudentAssignment(
            StudentAssignmentRow row)
    {
        AssignmentDeadlineDecision decision =
            _deadlinePolicy.Evaluate(
                row.Status,
                row.DeadlineUtc,
                row.AllowLateSubmission,
                DateTime.UtcNow);

        return new StudentAssignmentDto(
            row.Id,
            row.TeacherName,
            row.AcademicClassId,
            row.ClassName,
            row.ClassCode,
            row.SubjectId,
            row.SubjectName,
            row.SubjectCode,
            row.Title,
            row.Description,
            row.Instructions,
            row.DeadlineUtc,
            row.MaximumMarks,
            row.AllowResubmission,
            row.AllowLateSubmission,
            row.Status,
            row.SubmissionStatus,
            decision.IsPastDeadline,
            decision.CanSubmit,
            decision.WouldBeLate);
    }

    private void AddAudit(
        string userId,
        string action,
        Guid assignmentId,
        string description)
    {
        _dbContext.AuditLogs.Add(
            new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = "Assignment",
                EntityId =
                    assignmentId.ToString(),
                Description = description,
                CreatedAtUtc =
                    DateTime.UtcNow
            });
    }

    private static void ValidateRequest(
        Guid academicClassId,
        Guid subjectId,
        string title,
        string description,
        DateTime deadlineUtc,
        decimal maximumMarks)
    {
        if (academicClassId == Guid.Empty ||
            subjectId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "A valid class and subject are required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessRuleException(
                "Assignment title is required.");
        }

        if (string.IsNullOrWhiteSpace(
                description))
        {
            throw new BusinessRuleException(
                "Assignment description is required.");
        }

        if (deadlineUtc == default)
        {
            throw new BusinessRuleException(
                "A valid deadline is required.");
        }

        if (maximumMarks <= 0)
        {
            throw new BusinessRuleException(
                "Maximum marks must be greater than zero.");
        }
    }

    private static string?
        NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static DateTime NormalizeUtc(
        DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local =>
                value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc)
        };
    }

    private sealed record StudentAssignmentRow(
        Guid Id,
        string TeacherName,
        Guid AcademicClassId,
        string ClassName,
        string ClassCode,
        Guid SubjectId,
        string SubjectName,
        string SubjectCode,
        string Title,
        string Description,
        string? Instructions,
        DateTime DeadlineUtc,
        decimal MaximumMarks,
        bool AllowResubmission,
        bool AllowLateSubmission,
        AssignmentStatus Status,
        SubmissionStatus? SubmissionStatus);
}