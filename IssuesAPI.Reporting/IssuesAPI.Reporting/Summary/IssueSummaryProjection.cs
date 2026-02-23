using Marten.Events.Aggregation;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Summary;

public class IssueSummary
{
    public IssueId Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string? AssigneeName { get; set; }
    public UserId? AssigneeId { get; set; }
    public DateTimeOffset Created { get; set; }
    public int EventCount { get; set; }
}

public class IssueSummaryProjection : SingleStreamProjection<IssueSummary, IssueId>
{
    public IssueSummary Create(IssueCreated @event)
    {
        return new IssueSummary
        {
            Id = @event.Id,
            Title = @event.Title,
            Status = "Open",
            Created = @event.OpenedAt,
            EventCount = 1
        };
    }

    public void Apply(IssueAssigned @event, IssueSummary view)
    {
        view.AssigneeId = @event.AssigneeId;
        view.EventCount++;
    }

    public void Apply(IssueClosed @event, IssueSummary view)
    {
        view.Status = "Closed";
        view.EventCount++;
    }

    public void Apply(IssueOpened @event, IssueSummary view)
    {
        view.Status = "Open";
        view.EventCount++;
    }
}
