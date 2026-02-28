using Marten.Events.Aggregation;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Reporting.Summary;

public class IssueSummaryProjection : SingleStreamProjection<IssueSummary, IssueId>
{
    public IssueSummary Create(IssueCreated @event) => new()
    {
        Id = @event.Id,
        Title = @event.Title,
        Status = "Open",
        Created = @event.OpenedAt,
        EventCount = 1
    };

    public void Apply(IssueUnassigned @event, IssueSummary view)
    {
        view.AssigneeId = null;
        view.AssigneeName = null;
        view.EventCount++;
    }

    public void Apply(IssueAssigned @event, IssueSummary view)
    {
        view.AssigneeId = @event.AssigneeId;
        view.AssigneeName = @event.AssigneeName;
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
