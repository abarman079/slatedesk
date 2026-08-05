namespace SlateDesk.Domain.Entities;

public sealed class AcademicClass
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TeacherAllocation> TeacherAllocations { get; set; }
        = new List<TeacherAllocation>();

    public ICollection<StudentEnrollment> StudentEnrollments { get; set; }
        = new List<StudentEnrollment>();

    public ICollection<Assignment> Assignments { get; set; }
        = new List<Assignment>();
}