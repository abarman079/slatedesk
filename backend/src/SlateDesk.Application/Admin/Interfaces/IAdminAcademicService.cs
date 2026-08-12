using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Models;

namespace SlateDesk.Application.Admin.Interfaces;

public interface IAdminAcademicService
{
    Task<PagedResult<AcademicClassDto>>
        GetClassesAsync(
            AcademicListQuery query,
            CancellationToken cancellationToken);

    Task<AcademicClassDto> GetClassAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<AcademicClassDto> CreateClassAsync(
        SaveAcademicClassRequest request,
        string adminUserId,
        CancellationToken cancellationToken);

    Task<AcademicClassDto> UpdateClassAsync(
        Guid id,
        SaveAcademicClassRequest request,
        string adminUserId,
        CancellationToken cancellationToken);

    Task SetClassStatusAsync(
        Guid id,
        bool isActive,
        string adminUserId,
        CancellationToken cancellationToken);

    Task<PagedResult<SubjectDto>>
        GetSubjectsAsync(
            AcademicListQuery query,
            CancellationToken cancellationToken);

    Task<SubjectDto> GetSubjectAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<SubjectDto> CreateSubjectAsync(
        SaveSubjectRequest request,
        string adminUserId,
        CancellationToken cancellationToken);

    Task<SubjectDto> UpdateSubjectAsync(
        Guid id,
        SaveSubjectRequest request,
        string adminUserId,
        CancellationToken cancellationToken);

    Task SetSubjectStatusAsync(
        Guid id,
        bool isActive,
        string adminUserId,
        CancellationToken cancellationToken);
}