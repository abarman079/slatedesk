using SlateDesk.Domain.Enums;
using SlateDesk.Infrastructure.Assignments;

namespace SlateDesk.UnitTests.Assignments;

public sealed class AssignmentDeadlinePolicyTests
{
    private readonly AssignmentDeadlinePolicy _policy =
        new();

    [Fact]
    public void PublishedBeforeDeadline_AllowsSubmission()
    {
        DateTime now = DateTime.UtcNow;

        var result = _policy.Evaluate(
            AssignmentStatus.Published,
            now.AddHours(1),
            allowLateSubmission: false,
            now);

        Assert.False(result.IsPastDeadline);
        Assert.True(result.CanSubmit);
        Assert.False(result.WouldBeLate);
    }

    [Fact]
    public void PublishedAfterDeadline_WithoutLatePermission_BlocksSubmission()
    {
        DateTime now = DateTime.UtcNow;

        var result = _policy.Evaluate(
            AssignmentStatus.Published,
            now.AddMinutes(-1),
            allowLateSubmission: false,
            now);

        Assert.True(result.IsPastDeadline);
        Assert.False(result.CanSubmit);
        Assert.False(result.WouldBeLate);
    }

    [Fact]
    public void PublishedAfterDeadline_WithLatePermission_AllowsLateSubmission()
    {
        DateTime now = DateTime.UtcNow;

        var result = _policy.Evaluate(
            AssignmentStatus.Published,
            now.AddMinutes(-1),
            allowLateSubmission: true,
            now);

        Assert.True(result.IsPastDeadline);
        Assert.True(result.CanSubmit);
        Assert.True(result.WouldBeLate);
    }

    [Fact]
    public void DraftAssignment_NeverAllowsSubmission()
    {
        DateTime now = DateTime.UtcNow;

        var result = _policy.Evaluate(
            AssignmentStatus.Draft,
            now.AddHours(1),
            allowLateSubmission: true,
            now);

        Assert.False(result.CanSubmit);
    }
}
