using Marten;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using Wolverine.Reporting.Reports;

namespace Wolverine.Reporting.Handlers;

public static class IssueOpenedHandler
{
    public static async Task Handle(IssueOpened @event, IDocumentSession session)
    {
        var report = await session.LoadAsync<IssueReport>(@event.IssueId);
        if (report is null) return;

        report.Status = "Open";
        report.Closed = null;
        report.LastUpdated = @event.Reopened;
        report.EventCount++;

        session.Store(report);
    }
}
