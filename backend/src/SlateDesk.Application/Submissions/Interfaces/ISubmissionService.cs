using SlateDesk.Application.Common.Models;
using SlateDesk.Application.Submissions.Models;

namespace SlateDesk.Application.Submissions.Interfaces;

public interface ISubmissionService
{
    Task<PagedResult<StudentSubmissionDto>>
        GetStudentSubmissionsAsync(
            string studentId,
            SubmissionListQuery query,
            CancellationToken cancellationToken);

    Task<StudentSubmissionDto>
        GetStudentSubmissionAsync(
            Guid id,
            string studentId,
            CancellationToken cancellationToken);

    Task<StudentSubmissionDto>
        SaveDraftAsync(
            Guid assignmentId,
            string studentId,
            SaveSubmissionDraftRequest request,
            CancellationToken cancellationToken);

    Task<StudentSubmissionDto>
        UpdateStudentSubmissionAsync(
            Guid id,
            string studentId,
            UpdateSubmissionRequest request,
            CancellationToken cancellationToken);

    Task<StudentSubmissionDto>
        SubmitAsync(
            Guid id,
            string studentId,
            SubmitSubmissionRequest request,
            CancellationToken cancellationToken);

    Task<PagedResult<TeacherSubmissionDto>>
        GetTeacherAssignmentSubmissionsAsync(
            Guid assignmentId,
            string teacherId,
            SubmissionListQuery query,
            CancellationToken cancellationToken);

    Task<TeacherSubmissionDto>
        GetTeacherSubmissionAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken);

    Task<TeacherSubmissionDto>
        UpdateReviewStatusAsync(
            Guid id,
            string teacherId,
            UpdateReviewStatusRequest request,
            CancellationToken cancellationToken);

    Task<TeacherSubmissionDto>
        GradeAsync(
            Guid id,
            string teacherId,
            GradeSubmissionRequest request,
            CancellationToken cancellationToken);

    Task<PagedResult<StudentResultDto>>
        GetStudentResultsAsync(
            string studentId,
            SubmissionListQuery query,
            CancellationToken cancellationToken);
}
