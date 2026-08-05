using SlateDesk.Domain.Enums;

namespace SlateDesk.Domain.Entities;

public sealed class Assignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string TeacherId { get; set; } = string.Empty;

    public Guid AcademicClassId { get; set; }

    public Guid SubjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Instructions { get; set; }

    public DateTime DeadlineUtc { get; set; }

    public decimal MaximumMarks { get; set; }

    public bool AllowResubmission { get; set; }

    public bool AllowLateSubmission { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public DateTime? PublishedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public bool IsArchived { get; set; }

    public AcademicClass AcademicClass { get; set; } = null!;

    public Subject Subject { get; set; } = null!;

    public ICollection<Submission> Submissions { get; set; }
        = new List<Submission>();
}