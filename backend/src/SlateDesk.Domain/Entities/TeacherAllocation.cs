namespace SlateDesk.Domain.Entities;

public sealed class TeacherAllocation
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string TeacherId { get; set; } = string.Empty;

    public Guid AcademicClassId { get; set; }

    public Guid SubjectId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public AcademicClass AcademicClass { get; set; } = null!;

    public Subject Subject { get; set; } = null!;
}