using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Issues.Model;

namespace Wolverine.Issues.Issues.Assignment;

public static class AssignIssueEndpoint
{
    [EmptyResponse]
    [WolverinePut("/issues/{issueId}/assign")]
    public static async Task Assign(AssignIssue command, IDocumentSession session)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        var aggregate = stream.Aggregate!;

        if (aggregate.AssigneeId.HasValue)
        {
            stream.AppendOne(new IssueUnassigned(aggregate.Id, aggregate.AssigneeId.Value));
        }

        stream.AppendOne(new IssueAssigned(aggregate.Id, command.AssigneeId, aggregate.Title));
    }
}
