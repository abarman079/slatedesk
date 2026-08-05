using SlateDesk.Domain.Enums;

namespace SlateDesk.Domain.Entities;

public sealed class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssignmentId { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string AnswerText { get; set; } = string.Empty;

    public DateTime? SubmittedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;

    public decimal? MarksAwarded { get; set; }

    public string? TeacherFeedback { get; set; }

    public DateTime? GradedAtUtc { get; set; }

    public string? GradedByTeacherId { get; set; }

    // Mapped to PostgreSQL's hidden xmin system column.
    public uint Version { get; set; }

    public Assignment Assignment { get; set; } = null!;
}