using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Hubs;
using Wolverine.Issues.Issues.Model;

namespace Wolverine.Issues.Issues.Assignment;

public static class AssignIssueEndpoint
{
    [EmptyResponse]
    [WolverinePut("/api/issues/{issueId}/assign")]
    public static async Task Assign(AssignIssue command, IDocumentSession session, IssueEventBroadcaster broadcaster)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        var assigned = new IssueAssigned(stream.Aggregate!.Id, command.AssigneeId);
        stream.AppendOne(assigned);

        _ = broadcaster.Broadcast("IssueAssigned", assigned);
    }
}
