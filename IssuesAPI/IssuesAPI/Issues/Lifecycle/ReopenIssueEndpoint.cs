using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using Wolverine.Issues.Issues.Model;

namespace Wolverine.Issues.Issues.Lifecycle;

public static class ReopenIssueEndpoint
{
    [EmptyResponse]
    [WolverinePut("/issues/{issueId}/reopen")]
    public static async Task Reopen(ReopenIssue command, IDocumentSession session)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        var aggregate = stream.Aggregate!;
        var reopened = new IssueOpened(aggregate.Id, aggregate.AssigneeId ?? default, DateTimeOffset.UtcNow);
        stream.AppendOne(reopened);
    }
}
