using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Issues.Model;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Issues.Issues.Assignment;

public record UnassignIssue(IssueId IssueId);

public static class UnassignIssueEndpoint
{
    [WolverinePut("/issues/{issueId}/unassign")]
    public static async Task<IResult> Unassign(UnassignIssue command, IDocumentSession session)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        var aggregate = stream.Aggregate!;

        if (!aggregate.AssigneeId.HasValue)
            return Results.BadRequest("Issue is not currently assigned.");

        stream.AppendOne(new IssueUnassigned(aggregate.Id, aggregate.AssigneeId.Value));

        return Results.NoContent();
    }
}
