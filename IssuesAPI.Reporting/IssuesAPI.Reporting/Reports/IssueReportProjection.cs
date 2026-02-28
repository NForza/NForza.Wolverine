using Marten.Events.Projections;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Reports;

public class IssueReportProjection : MultiStreamProjection<AssigneeIssueReport, UserId>
{
    public IssueReportProjection()
    {
        Identity<IssueAssigned>(x => x.AssigneeId);
        Identity<IssueUnassigned>(x => x.AssigneeId);
        Identity<IssueClosed>(x => x.AssigneeId);
        Identity<IssueOpened>(x => x.AssigneeId);
    }

    public AssigneeIssueReport Create(IssueAssigned @event) => new()
    {
        Id = @event.AssigneeId,
        AssigneeName = @event.AssigneeName,
        Issues =
        [
            new AssigneeIssueItem
            {
                IssueId = @event.IssueId,
                Title = @event.Title,
                Status = "Open"
            }
        ]
    };

    public void Apply(IssueAssigned @event, AssigneeIssueReport view)
    {
        if (view.Issues.All(i => i.IssueId != @event.IssueId))
        {
            view.Issues.Add(new AssigneeIssueItem
            {
                IssueId = @event.IssueId,
                Title = @event.Title,
                Status = "Open"
            });
        }
    }

    public void Apply(IssueUnassigned @event, AssigneeIssueReport view) =>
        view.Issues.RemoveAll(i => i.IssueId == @event.IssueId);

    public void Apply(IssueClosed @event, AssigneeIssueReport view)
    {
        var issue = view.Issues.FirstOrDefault(i => i.IssueId == @event.IssueId);
        issue?.Status = "Closed";
    }

    public void Apply(IssueOpened @event, AssigneeIssueReport view)
    {
        var issue = view.Issues.FirstOrDefault(i => i.IssueId == @event.IssueId);
        issue?.Status = "Open";
    }
}
