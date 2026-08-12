using SlateDesk.Application.Assignments.Models;
using SlateDesk.Application.Common.Models;

namespace SlateDesk.Application.Assignments.Interfaces;

public interface IAssignmentService
{
    Task<PagedResult<TeacherAssignmentDto>>
        GetTeacherAssignmentsAsync(
            string teacherId,
            TeacherAssignmentListQuery query,
            CancellationToken cancellationToken);

    Task<TeacherAssignmentDto>
        GetTeacherAssignmentAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken);

    Task<TeacherAssignmentDto>
        CreateAssignmentAsync(
            string teacherId,
            CreateAssignmentRequest request,
            CancellationToken cancellationToken);

    Task<TeacherAssignmentDto>
        UpdateAssignmentAsync(
            Guid id,
            string teacherId,
            UpdateAssignmentRequest request,
            CancellationToken cancellationToken);

    Task DeleteAssignmentAsync(
        Guid id,
        string teacherId,
        CancellationToken cancellationToken);

    Task<TeacherAssignmentDto>
        PublishAssignmentAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken);

    Task<TeacherAssignmentDto>
        CloseAssignmentAsync(
            Guid id,
            string teacherId,
            CancellationToken cancellationToken);

    Task<PagedResult<StudentAssignmentDto>>
        GetStudentAssignmentsAsync(
            string studentId,
            StudentAssignmentListQuery query,
            CancellationToken cancellationToken);

    Task<StudentAssignmentDto>
        GetStudentAssignmentAsync(
            Guid id,
            string studentId,
            CancellationToken cancellationToken);
}