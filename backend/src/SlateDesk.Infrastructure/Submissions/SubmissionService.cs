using Microsoft.EntityFrameworkCore;
using SlateDesk.Application.Assignments.Interfaces;
using SlateDesk.Application.Assignments.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Application.Submissions.Interfaces;
using SlateDesk.Application.Submissions.Models;
using SlateDesk.Domain.Entities;
using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.Submissions;

public sealed class SubmissionService
    : ISubmissionService
{
    private readonly ApplicationDbContext _dbContext;

    private readonly IAssignmentDeadlinePolicy
        _deadlinePolicy;

    public SubmissionService(
        ApplicationDbContext dbContext,
        IAssignmentDeadlinePolicy deadlinePolicy)
    {
        _dbContext = dbContext;
        _deadlinePolicy = deadlinePolicy;
    }

    // =========================================================
    // STUDENT
    // =========================================================

    public async Task<
        PagedResult<StudentSubmissionDto>>
        GetStudentSubmissionsAsync(
            string studentId,
            SubmissionListQuery query,
            CancellationToken cancellationToken)
    {
        IQueryable<Submission> submissions =
            _dbContext.Submissions
                .AsNoTracking()
                .Where(submission =>
                    submission.StudentId == studentId);

        if (query.Status.HasValue)
        {
            submissions = submissions.Where(
                submission =>
                    submission.Status ==
                    query.Status.Value);
        }

        int totalItems =
            await submissions.CountAsync(
                cancellationToken);

        StudentSubmissionRow[] rows =
            await submissions
                .OrderByDescending(
                    submission =>
                        submission.UpdatedAtUtc)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(submission =>
                    new StudentSubmissionRow(
                        submission.Id,
                        submission.AssignmentId,
                        submission.Assignment.Title,
                        submission.Assignment.Subject.Code,
                        submission.Assignment.DeadlineUtc,
                        submission.Assignment.MaximumMarks,
                        submission.Assignment
                            .AllowResubmission,
                        submission.Assignment
                            .AllowLateSubmission,
                        submission.Assignment.Status,
                        submission.AnswerText,
                        submission.SubmittedAtUtc,
                        submission.UpdatedAtUtc,
                        submission.Status,
                        submission.MarksAwarded,
                        submission.TeacherFeedback,
                        submission.GradedAtUtc,
                        submission.Version))
                .ToArrayAsync(cancellationToken);

        StudentSubmissionDto[] items =
            rows.Select(MapStudentSubmission)
                .ToArray();

        return PagedResult<
            StudentSubmissionDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems);
    }

    public async Task<StudentSubmissionDto>
        GetStudentSubmissionAsync(
            Guid id,
            string studentId,
            CancellationToken cancellationToken)
    {
        StudentSubmissionRow? row =
            await _dbContext.Submissions
                .AsNoTracking()
                .Where(submission =>
                    submission.Id == id &&
                    submission.StudentId ==
                    studentId)
                .Select(submission =>
                    new StudentSubmissionRow(
                        submission.Id,
                        submission.AssignmentId,
                        submission.Assignment.Title,
                        submission.Assignment.Subject.Code,
                        submission.Assignment.DeadlineUtc,
                        submission.Assignment.MaximumMarks,
                        submission.Assignment
                            .AllowResubmission,
                        submission.Assignment
                            .AllowLateSubmission,
                        submission.Assignment.Status,
                        submission.AnswerText,
                        submission.SubmittedAtUtc,
                        submission.UpdatedAtUtc,
                        submission.Status,
                        submission.MarksAwarded,
                        submission.TeacherFeedback,
                        submission.GradedAtUtc,
                        submission.Version))
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (row is null)
        {
            throw new ResourceNotFoundException(
                "The submission was not found.");
        }

        return MapStudentSubmission(row);
    }

    public async Task<StudentSubmissionDto>
        SaveDraftAsync(
            Guid assignmentId,
            string studentId,
            SaveSubmissionDraftRequest request,
            CancellationToken cancellationToken)
    {
        Assignment assignment =
            await GetAccessibleStudentAssignmentAsync(
                assignmentId,
                studentId,
                cancellationToken);

        AssignmentDeadlineDecision decision =
            _deadlinePolicy.Evaluate(
                assignment.Status,
                assignment.DeadlineUtc,
                assignment.AllowLateSubmission,
                DateTime.UtcNow);

        if (!decision.CanSubmit)
        {
            throw new BusinessRuleException(
                "A submission can no longer be created for this assignment.");
        }

        bool existingSubmission =
            await _dbContext.Submissions
                .AnyAsync(
                    submission =>
                        submission.AssignmentId ==
                            assignmentId &&
                        submission.StudentId ==
                            studentId,
                    cancellationToken);

        if (existingSubmission)
        {
            throw new ConflictException(
                "A submission already exists for this assignment. Update the existing submission instead.");
        }

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText =
                request.AnswerText?.Trim()
                ?? string.Empty,
            Status = SubmissionStatus.Draft,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Submissions.Add(submission);

        AddAudit(
            studentId,
            "SubmissionDraftCreated",
            submission.Id,
            $"Created a draft submission for assignment '{assignment.Title}'.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetStudentSubmissionAsync(
            submission.Id,
            studentId,
            cancellationToken);
    }

    public async Task<StudentSubmissionDto>
        UpdateStudentSubmissionAsync(
            Guid id,
            string studentId,
            UpdateSubmissionRequest request,
            CancellationToken cancellationToken)
    {
        ValidateVersion(request.Version);

        Submission submission =
            await GetStudentSubmissionEntityAsync(
                id,
                studentId,
                cancellationToken);

        Assignment assignment =
            submission.Assignment;

        EnsureStudentCanModify(
            submission,
            assignment);

        AssignmentDeadlineDecision decision =
            _deadlinePolicy.Evaluate(
                assignment.Status,
                assignment.DeadlineUtc,
                assignment.AllowLateSubmission,
                DateTime.UtcNow);

        if (!decision.CanSubmit)
        {
            throw new BusinessRuleException(
                "The submission can no longer be changed because the submission window is closed.");
        }

        SetOriginalVersion(
            submission,
            request.Version);

        submission.AnswerText =
            request.AnswerText?.Trim()
            ?? string.Empty;

        submission.UpdatedAtUtc =
            DateTime.UtcNow;

        if (submission.Status is
            SubmissionStatus.Submitted
            or SubmissionStatus.Late
            or SubmissionStatus.NeedsRevision)
        {
            submission.Status =
                SubmissionStatus.Draft;
        }

        AddAudit(
            studentId,
            "SubmissionUpdated",
            submission.Id,
            $"Updated submission for assignment '{assignment.Title}'.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetStudentSubmissionAsync(
            submission.Id,
            studentId,
            cancellationToken);
    }

    public async Task<StudentSubmissionDto>
        SubmitAsync(
            Guid id,
            string studentId,
            SubmitSubmissionRequest request,
            CancellationToken cancellationToken)
    {
        ValidateVersion(request.Version);

        Submission submission =
            await GetStudentSubmissionEntityAsync(
                id,
                studentId,
                cancellationToken);

        Assignment assignment =
            submission.Assignment;

        if (string.IsNullOrWhiteSpace(
                submission.AnswerText))
        {
            throw new BusinessRuleException(
                "The answer cannot be blank when submitting.");
        }

        if (submission.Status is
            SubmissionStatus.UnderReview
            or SubmissionStatus.Graded)
        {
            throw new BusinessRuleException(
                "This submission is currently locked from student changes.");
        }

        bool previouslySubmitted =
            submission.SubmittedAtUtc.HasValue;

        if (previouslySubmitted &&
            !assignment.AllowResubmission)
        {
            throw new BusinessRuleException(
                "Resubmission is not allowed for this assignment.");
        }

        AssignmentDeadlineDecision decision =
            _deadlinePolicy.Evaluate(
                assignment.Status,
                assignment.DeadlineUtc,
                assignment.AllowLateSubmission,
                DateTime.UtcNow);

        if (!decision.CanSubmit)
        {
            throw new BusinessRuleException(
                "The submission deadline has passed and late submission is not allowed.");
        }

        SetOriginalVersion(
            submission,
            request.Version);

        DateTime now = DateTime.UtcNow;

        submission.Status =
            decision.WouldBeLate
                ? SubmissionStatus.Late
                : SubmissionStatus.Submitted;

        submission.SubmittedAtUtc = now;
        submission.UpdatedAtUtc = now;

        AddAudit(
            studentId,
            previouslySubmitted
                ? "SubmissionResubmitted"
                : "SubmissionSubmitted",
            submission.Id,
            previouslySubmitted
                ? $"Resubmitted work for assignment '{assignment.Title}'."
                : $"Submitted work for assignment '{assignment.Title}'.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetStudentSubmissionAsync(
            submission.Id,
            studentId,
            cancellationToken);
    }

    // =========================================================
    // TEACHER
    // =========================================================

    public async Task<
        PagedResult<TeacherSubmissionDto>>
        GetTeacherAssignmentSubmissionsAsync(
            Guid assignmentId,
            string teacherId,
            SubmissionListQuery query,
            CancellationToken cancellationToken)
    {
        await EnsureTeacherOwnsAssignmentAsync(
            assignmentId,
            teacherId,
            cancellationToken);

        IQueryable<Submission> submissions =
            _dbContext.Submissions
                .AsNoTracking()
                .Where(submission =>
                    submission.AssignmentId ==
                        assignmentId);

        if (query.Status.HasValue)
        {
            submissions = submissions.Where(
                submission =>
                    submission.Status ==
                    query.Status.Value);
        }

        int totalItems =
            await submissions.CountAsync(
                cancellationToken);

        TeacherSubmissionDto[] items =
            await submissions
                .OrderByDescending(
                    submission =>
                        submission.SubmittedAtUtc)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(submission =>
                    new TeacherSubmissionDto(
                        submission.Id,
                        submission.AssignmentId,
                        submission.Assignment.Title,
                        submission.StudentId,
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                submission.StudentId)
                            .Select(user =>
                                user.FullName)
                            .FirstOrDefault() ??
                            "Student",
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                submission.StudentId)
                            .Select(user =>
                                user.Email)
                            .FirstOrDefault() ??
                            string.Empty,
                        submission.AnswerText,
                        submission.SubmittedAtUtc,
                        submission.UpdatedAtUtc,
                        submission.Status,
                        submission.MarksAwarded,
                        submission.TeacherFeedback,
                        submission.GradedAtUtc,
                        submission.Version,
                        submission.Status ==
                            SubmissionStatus.Late))
                .ToArrayAsync(
                    cancellationToken);

        return PagedResult<
            TeacherSubmissionDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems);
    }

    public async Task<TeacherSubmissionDto>
        GetTeacherSubmissionAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken)
    {
        TeacherSubmissionDto? result =
            await _dbContext.Submissions
                .AsNoTracking()
                .Where(submission =>
                    submission.Id == id &&
                    submission.Assignment.TeacherId ==
                        teacherId)
                .Select(submission =>
                    new TeacherSubmissionDto(
                        submission.Id,
                        submission.AssignmentId,
                        submission.Assignment.Title,
                        submission.StudentId,
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                submission.StudentId)
                            .Select(user =>
                                user.FullName)
                            .FirstOrDefault() ??
                            "Student",
                        _dbContext.Users
                            .IgnoreQueryFilters()
                            .Where(user =>
                                user.Id ==
                                submission.StudentId)
                            .Select(user =>
                                user.Email)
                            .FirstOrDefault() ??
                            string.Empty,
                        submission.AnswerText,
                        submission.SubmittedAtUtc,
                        submission.UpdatedAtUtc,
                        submission.Status,
                        submission.MarksAwarded,
                        submission.TeacherFeedback,
                        submission.GradedAtUtc,
                        submission.Version,
                        submission.Status ==
                            SubmissionStatus.Late))
                .SingleOrDefaultAsync(
                    cancellationToken);

        return result ??
            throw new ResourceNotFoundException(
                "The submission was not found.");
    }

    public async Task<TeacherSubmissionDto>
        UpdateReviewStatusAsync(
            Guid id,
            string teacherId,
            UpdateReviewStatusRequest request,
            CancellationToken cancellationToken)
    {
        ValidateVersion(request.Version);

        if (request.Status is not
            SubmissionStatus.UnderReview and not
            SubmissionStatus.NeedsRevision)
        {
            throw new BusinessRuleException(
                "Review status must be UnderReview or NeedsRevision.");
        }

        Submission submission =
            await GetTeacherSubmissionEntityAsync(
                id,
                teacherId,
                cancellationToken);

        if (submission.Status ==
            SubmissionStatus.Draft)
        {
            throw new BusinessRuleException(
                "A draft submission cannot be reviewed.");
        }

        SetOriginalVersion(
            submission,
            request.Version);

        submission.Status = request.Status;
        submission.UpdatedAtUtc =
            DateTime.UtcNow;

        AddAudit(
            teacherId,
            "SubmissionReviewStatusChanged",
            submission.Id,
            $"Changed review status to {request.Status}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetTeacherSubmissionAsync(
            id,
            teacherId,
            cancellationToken);
    }

    public async Task<TeacherSubmissionDto>
        GradeAsync(
            Guid id,
            string teacherId,
            GradeSubmissionRequest request,
            CancellationToken cancellationToken)
    {
        ValidateVersion(request.Version);

        Submission submission =
            await GetTeacherSubmissionEntityAsync(
                id,
                teacherId,
                cancellationToken);

        if (submission.Status ==
            SubmissionStatus.Draft)
        {
            throw new BusinessRuleException(
                "A draft submission cannot be graded.");
        }

        if (request.MarksAwarded < 0)
        {
            throw new BusinessRuleException(
                "Marks cannot be negative.");
        }

        if (request.MarksAwarded >
            submission.Assignment.MaximumMarks)
        {
            throw new BusinessRuleException(
                $"Marks cannot exceed the assignment maximum of {submission.Assignment.MaximumMarks}.");
        }

        SetOriginalVersion(
            submission,
            request.Version);

        DateTime now = DateTime.UtcNow;

        submission.MarksAwarded =
            request.MarksAwarded;

        submission.TeacherFeedback =
            string.IsNullOrWhiteSpace(
                request.TeacherFeedback)
                ? null
                : request.TeacherFeedback.Trim();

        submission.GradedAtUtc = now;
        submission.GradedByTeacherId =
            teacherId;

        submission.Status =
            SubmissionStatus.Graded;

        submission.UpdatedAtUtc = now;

        AddAudit(
            teacherId,
            "SubmissionGraded",
            submission.Id,
            $"Graded submission with {request.MarksAwarded}/{submission.Assignment.MaximumMarks} marks.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetTeacherSubmissionAsync(
            id,
            teacherId,
            cancellationToken);
    }

    // =========================================================
    // RESULTS
    // =========================================================

    public async Task<PagedResult<StudentResultDto>>
        GetStudentResultsAsync(
            string studentId,
            SubmissionListQuery query,
            CancellationToken cancellationToken)
    {
        IQueryable<Submission> submissions =
            _dbContext.Submissions
                .AsNoTracking()
                .Where(submission =>
                    submission.StudentId ==
                        studentId &&
                    submission.Status ==
                        SubmissionStatus.Graded &&
                    submission.MarksAwarded != null);

        int totalItems =
            await submissions.CountAsync(
                cancellationToken);

        StudentResultDto[] items =
            await submissions
                .OrderByDescending(
                    submission =>
                        submission.GradedAtUtc)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(submission =>
                    new StudentResultDto(
                        submission.Id,
                        submission.AssignmentId,
                        submission.Assignment.Title,
                        submission.Assignment
                            .Subject.Name,
                        submission.Assignment
                            .Subject.Code,
                        submission.MarksAwarded!.Value,
                        submission.Assignment
                            .MaximumMarks,
                        submission.TeacherFeedback,
                        submission.GradedAtUtc,
                        submission.Status))
                .ToArrayAsync(
                    cancellationToken);

        return PagedResult<
            StudentResultDto>.Create(
                items,
                query.Page,
                query.PageSize,
                totalItems);
    }

    // =========================================================
    // PRIVATE HELPERS
    // =========================================================

    private async Task<Assignment>
        GetAccessibleStudentAssignmentAsync(
            Guid assignmentId,
            string studentId,
            CancellationToken cancellationToken)
    {
        Guid? activeClassId =
            await _dbContext
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

        if (!activeClassId.HasValue)
        {
            throw new ResourceNotFoundException(
                "The assignment was not found.");
        }

        Assignment? assignment =
            await _dbContext.Assignments
                .SingleOrDefaultAsync(
                    item =>
                        item.Id ==
                            assignmentId &&
                        item.AcademicClassId ==
                            activeClassId.Value &&
                        (
                            item.Status ==
                                AssignmentStatus.Published ||
                            (
                                item.Status ==
                                    AssignmentStatus.Closed &&
                                item.AllowLateSubmission
                            )
                        ),
                    cancellationToken);

        return assignment ??
            throw new ResourceNotFoundException(
                "The assignment was not found.");
    }

    private async Task<Submission>
        GetStudentSubmissionEntityAsync(
            Guid id,
            string studentId,
            CancellationToken cancellationToken)
    {
        Submission? submission =
            await _dbContext.Submissions
                .Include(item =>
                    item.Assignment)
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == id &&
                        item.StudentId ==
                            studentId,
                    cancellationToken);

        return submission ??
            throw new ResourceNotFoundException(
                "The submission was not found.");
    }

    private async Task<Submission>
        GetTeacherSubmissionEntityAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken)
    {
        Submission? submission =
            await _dbContext.Submissions
                .Include(item =>
                    item.Assignment)
                .SingleOrDefaultAsync(
                    item =>
                        item.Id == id &&
                        item.Assignment.TeacherId ==
                            teacherId,
                    cancellationToken);

        return submission ??
            throw new ResourceNotFoundException(
                "The submission was not found.");
    }

    private async Task
        EnsureTeacherOwnsAssignmentAsync(
            Guid assignmentId,
            string teacherId,
            CancellationToken cancellationToken)
    {
        bool ownsAssignment =
            await _dbContext.Assignments
                .AnyAsync(
                    assignment =>
                        assignment.Id ==
                            assignmentId &&
                        assignment.TeacherId ==
                            teacherId,
                    cancellationToken);

        if (!ownsAssignment)
        {
            throw new ResourceNotFoundException(
                "The assignment was not found.");
        }
    }

    private void EnsureStudentCanModify(
        Submission submission,
        Assignment assignment)
    {
        if (submission.Status is
            SubmissionStatus.UnderReview
            or SubmissionStatus.Graded)
        {
            throw new BusinessRuleException(
                "This submission is locked because it is under review or already graded.");
        }

        if (submission.Status !=
                SubmissionStatus.Draft &&
            !assignment.AllowResubmission)
        {
            throw new BusinessRuleException(
                "Resubmission is not allowed for this assignment.");
        }
    }

    private StudentSubmissionDto
        MapStudentSubmission(
            StudentSubmissionRow row)
    {
        AssignmentDeadlineDecision decision =
            _deadlinePolicy.Evaluate(
                row.AssignmentStatus,
                row.DeadlineUtc,
                row.AllowLateSubmission,
                DateTime.UtcNow);

        bool statusAllowsEdit =
            row.Status is not
                SubmissionStatus.UnderReview
            and not SubmissionStatus.Graded;

        bool policyAllowsEdit =
            row.Status == SubmissionStatus.Draft ||
            row.AllowResubmission;

        bool canEdit =
            decision.CanSubmit &&
            statusAllowsEdit &&
            policyAllowsEdit;

        bool canSubmit =
            canEdit;

        return new StudentSubmissionDto(
            row.Id,
            row.AssignmentId,
            row.AssignmentTitle,
            row.SubjectCode,
            row.DeadlineUtc,
            row.MaximumMarks,
            row.AnswerText,
            row.SubmittedAtUtc,
            row.UpdatedAtUtc,
            row.Status,
            row.MarksAwarded,
            row.TeacherFeedback,
            row.GradedAtUtc,
            row.Version,
            canEdit,
            canSubmit);
    }

    private void SetOriginalVersion(
        Submission submission,
        uint version)
    {
        _dbContext.Entry(submission)
            .Property(item => item.Version)
            .OriginalValue = version;
    }

    private static void ValidateVersion(
        uint version)
    {
        if (version == 0)
        {
            throw new BusinessRuleException(
                "A valid submission version is required.");
        }
    }

    private void AddAudit(
        string userId,
        string action,
        Guid submissionId,
        string description)
    {
        _dbContext.AuditLogs.Add(
            new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = "Submission",
                EntityId =
                    submissionId.ToString(),
                Description = description,
                CreatedAtUtc =
                    DateTime.UtcNow
            });
    }

    private sealed record StudentSubmissionRow(
        Guid Id,
        Guid AssignmentId,
        string AssignmentTitle,
        string SubjectCode,
        DateTime DeadlineUtc,
        decimal MaximumMarks,
        bool AllowResubmission,
        bool AllowLateSubmission,
        AssignmentStatus AssignmentStatus,
        string AnswerText,
        DateTime? SubmittedAtUtc,
        DateTime UpdatedAtUtc,
        SubmissionStatus Status,
        decimal? MarksAwarded,
        string? TeacherFeedback,
        DateTime? GradedAtUtc,
        uint Version);
}
