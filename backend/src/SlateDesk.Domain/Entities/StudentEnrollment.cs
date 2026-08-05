namespace SlateDesk.Domain.Entities;

public sealed class StudentEnrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string StudentId { get; set; } = string.Empty;

    public Guid AcademicClassId { get; set; }

    public DateTime EnrolledAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public AcademicClass AcademicClass { get; set; } = null!;
}