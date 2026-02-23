using Marten;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Reporting.Reports;

namespace Wolverine.Reporting.Handlers;

public static class IssueCreatedHandler
{
    public static void Handle(IssueCreated @event, IDocumentSession session)
    {
        session.Store(new IssueReport
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            OriginatorId = @event.OriginatorId,
            Status = "Open",
            Created = @event.OpenedAt,
            LastUpdated = @event.OpenedAt,
            EventCount = 1
        });
    }
}
