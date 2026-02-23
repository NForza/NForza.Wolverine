using Marten;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Reporting.Reports;

namespace Wolverine.Reporting.Handlers;

public static class IssueAssignedHandler
{
    public static async Task Handle(IssueAssigned @event, IDocumentSession session)
    {
        var report = await session.LoadAsync<IssueReport>(@event.IssueId);
        if (report is null) return;

        report.AssigneeId = @event.AssigneeId;
        report.LastUpdated = DateTimeOffset.UtcNow;
        report.EventCount++;

        session.Store(report);
    }
}
