using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Issues.Model;

namespace Wolverine.Issues.Issues.Assignment;

public static class AssignIssueEndpoint
{
    [EmptyResponse]
    [WolverinePut("/api/issues/{issueId}/assign")]
    public static async Task Assign(AssignIssue command, IDocumentSession session)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        stream.AppendOne(new IssueAssigned(stream.Aggregate!.Id, command.AssigneeId));
    }
}
