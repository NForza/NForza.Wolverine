using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using Wolverine.Issues.Hubs;
using Wolverine.Issues.Issues.Model;

namespace Wolverine.Issues.Issues.Lifecycle;

public static class CloseIssueEndpoint
{
    [EmptyResponse]
    [WolverinePut("/issues/{issueId}/close")]
    public static async Task Close(CloseIssue command, IDocumentSession session, IssueEventBroadcaster broadcaster)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        var closed = new IssueClosed(stream.Aggregate!.Id, DateTimeOffset.UtcNow);
        stream.AppendOne(closed);

        _ = broadcaster.Broadcast("IssueClosed", closed);
    }
}
