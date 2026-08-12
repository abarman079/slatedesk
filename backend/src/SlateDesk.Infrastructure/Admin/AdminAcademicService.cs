using Microsoft.EntityFrameworkCore;
using SlateDesk.Application.Admin.Interfaces;
using SlateDesk.Application.Admin.Models;
using SlateDesk.Application.Common.Exceptions;
using SlateDesk.Application.Common.Models;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Persistence;

namespace SlateDesk.Infrastructure.Admin;

public sealed class AdminAcademicService
    : IAdminAcademicService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminAcademicService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AcademicClassDto>>
        GetClassesAsync(
            AcademicListQuery query,
            CancellationToken cancellationToken)
    {
        IQueryable<AcademicClass> classes =
            _dbContext.AcademicClasses
                .IgnoreQueryFilters()
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            classes = classes.Where(item =>
                item.Name.ToLower().Contains(search) ||
                item.Code.ToLower().Contains(search) ||
                item.AcademicYear.ToLower()
                    .Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            classes = classes.Where(item =>
                item.IsActive ==
                query.IsActive.Value);
        }

        int count =
            await classes.CountAsync(
                cancellationToken);

        AcademicClassDto[] items =
            await classes
                .OrderBy(item => item.Code)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(item =>
                    new AcademicClassDto(
                        item.Id,
                        item.Name,
                        item.Code,
                        item.AcademicYear,
                        item.Description,
                        item.IsActive,
                        item.CreatedAtUtc))
                .ToArrayAsync(cancellationToken);

        return PagedResult<AcademicClassDto>.Create(
            items,
            query.Page,
            query.PageSize,
            count);
    }

    public async Task<AcademicClassDto>
        GetClassAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        return await _dbContext.AcademicClasses
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item =>
                new AcademicClassDto(
                    item.Id,
                    item.Name,
                    item.Code,
                    item.AcademicYear,
                    item.Description,
                    item.IsActive,
                    item.CreatedAtUtc))
            .SingleOrDefaultAsync(
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The academic class was not found.");
    }

    public async Task<AcademicClassDto>
        CreateClassAsync(
            SaveAcademicClassRequest request,
            string adminUserId,
            CancellationToken cancellationToken)
    {
        string code = NormalizeCode(request.Code);

        bool duplicate =
            await _dbContext.AcademicClasses
                .IgnoreQueryFilters()
                .AnyAsync(
                    item => item.Code == code,
                    cancellationToken);

        if (duplicate)
        {
            throw new ConflictException(
                $"Academic class code '{code}' already exists.");
        }

        var entity = new AcademicClass
        {
            Name = request.Name.Trim(),
            Code = code,
            AcademicYear =
                request.AcademicYear.Trim(),
            Description =
                NormalizeOptionalText(
                    request.Description),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.AcademicClasses.Add(entity);

        AddAudit(
            adminUserId,
            "ClassCreated",
            "AcademicClass",
            entity.Id.ToString(),
            $"Created class {entity.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapClass(entity);
    }

    public async Task<AcademicClassDto>
        UpdateClassAsync(
            Guid id,
            SaveAcademicClassRequest request,
            string adminUserId,
            CancellationToken cancellationToken)
    {
        AcademicClass entity =
            await GetClassEntityAsync(
                id,
                cancellationToken);

        string code = NormalizeCode(request.Code);

        bool duplicate =
            await _dbContext.AcademicClasses
                .IgnoreQueryFilters()
                .AnyAsync(
                    item =>
                        item.Id != id &&
                        item.Code == code,
                    cancellationToken);

        if (duplicate)
        {
            throw new ConflictException(
                $"Academic class code '{code}' already exists.");
        }

        entity.Name = request.Name.Trim();
        entity.Code = code;
        entity.AcademicYear =
            request.AcademicYear.Trim();
        entity.Description =
            NormalizeOptionalText(
                request.Description);

        AddAudit(
            adminUserId,
            "ClassUpdated",
            "AcademicClass",
            entity.Id.ToString(),
            $"Updated class {entity.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapClass(entity);
    }

    public async Task SetClassStatusAsync(
        Guid id,
        bool isActive,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        AcademicClass entity =
            await GetClassEntityAsync(
                id,
                cancellationToken);

        entity.IsActive = isActive;

        AddAudit(
            adminUserId,
            isActive
                ? "ClassActivated"
                : "ClassDeactivated",
            "AcademicClass",
            entity.Id.ToString(),
            isActive
                ? $"Activated class {entity.Code}."
                : $"Deactivated class {entity.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<PagedResult<SubjectDto>>
        GetSubjectsAsync(
            AcademicListQuery query,
            CancellationToken cancellationToken)
    {
        IQueryable<Subject> subjects =
            _dbContext.Subjects
                .IgnoreQueryFilters()
                .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string search =
                query.Search.Trim().ToLower();

            subjects = subjects.Where(item =>
                item.Name.ToLower().Contains(search) ||
                item.Code.ToLower().Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            subjects = subjects.Where(item =>
                item.IsActive ==
                query.IsActive.Value);
        }

        int count =
            await subjects.CountAsync(
                cancellationToken);

        SubjectDto[] items =
            await subjects
                .OrderBy(item => item.Code)
                .Skip(
                    (query.Page - 1) *
                    query.PageSize)
                .Take(query.PageSize)
                .Select(item =>
                    new SubjectDto(
                        item.Id,
                        item.Name,
                        item.Code,
                        item.Description,
                        item.IsActive,
                        item.CreatedAtUtc))
                .ToArrayAsync(cancellationToken);

        return PagedResult<SubjectDto>.Create(
            items,
            query.Page,
            query.PageSize,
            count);
    }

    public async Task<SubjectDto> GetSubjectAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Subjects
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item =>
                new SubjectDto(
                    item.Id,
                    item.Name,
                    item.Code,
                    item.Description,
                    item.IsActive,
                    item.CreatedAtUtc))
            .SingleOrDefaultAsync(
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The subject was not found.");
    }

    public async Task<SubjectDto> CreateSubjectAsync(
        SaveSubjectRequest request,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        string code = NormalizeCode(request.Code);

        bool duplicate =
            await _dbContext.Subjects
                .IgnoreQueryFilters()
                .AnyAsync(
                    item => item.Code == code,
                    cancellationToken);

        if (duplicate)
        {
            throw new ConflictException(
                $"Subject code '{code}' already exists.");
        }

        var entity = new Subject
        {
            Name = request.Name.Trim(),
            Code = code,
            Description =
                NormalizeOptionalText(
                    request.Description),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Subjects.Add(entity);

        AddAudit(
            adminUserId,
            "SubjectCreated",
            "Subject",
            entity.Id.ToString(),
            $"Created subject {entity.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapSubject(entity);
    }

    public async Task<SubjectDto> UpdateSubjectAsync(
        Guid id,
        SaveSubjectRequest request,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        Subject entity =
            await GetSubjectEntityAsync(
                id,
                cancellationToken);

        string code = NormalizeCode(request.Code);

        bool duplicate =
            await _dbContext.Subjects
                .IgnoreQueryFilters()
                .AnyAsync(
                    item =>
                        item.Id != id &&
                        item.Code == code,
                    cancellationToken);

        if (duplicate)
        {
            throw new ConflictException(
                $"Subject code '{code}' already exists.");
        }

        entity.Name = request.Name.Trim();
        entity.Code = code;
        entity.Description =
            NormalizeOptionalText(
                request.Description);

        AddAudit(
            adminUserId,
            "SubjectUpdated",
            "Subject",
            entity.Id.ToString(),
            $"Updated subject {entity.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return MapSubject(entity);
    }

    public async Task SetSubjectStatusAsync(
        Guid id,
        bool isActive,
        string adminUserId,
        CancellationToken cancellationToken)
    {
        Subject entity =
            await GetSubjectEntityAsync(
                id,
                cancellationToken);

        entity.IsActive = isActive;

        AddAudit(
            adminUserId,
            isActive
                ? "SubjectActivated"
                : "SubjectDeactivated",
            "Subject",
            entity.Id.ToString(),
            isActive
                ? $"Activated subject {entity.Code}."
                : $"Deactivated subject {entity.Code}.");

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<AcademicClass>
        GetClassEntityAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        return await _dbContext.AcademicClasses
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The academic class was not found.");
    }

    private async Task<Subject>
        GetSubjectEntityAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        return await _dbContext.Subjects
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                item => item.Id == id,
                cancellationToken)
            ?? throw new ResourceNotFoundException(
                "The subject was not found.");
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

    private static string NormalizeCode(
        string value)
    {
        return value
            .Trim()
            .ToUpperInvariant();
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static AcademicClassDto MapClass(
        AcademicClass entity)
    {
        return new AcademicClassDto(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.AcademicYear,
            entity.Description,
            entity.IsActive,
            entity.CreatedAtUtc);
    }

    private static SubjectDto MapSubject(
        Subject entity)
    {
        return new SubjectDto(
            entity.Id,
            entity.Name,
            entity.Code,
            entity.Description,
            entity.IsActive,
            entity.CreatedAtUtc);
    }
}