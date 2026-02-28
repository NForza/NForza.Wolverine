using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Issues.Model;
using Wolverine.Issues.Users;

namespace Wolverine.Issues.Issues.Assignment;

public static class AssignIssueEndpoint
{
    [WolverinePut("/issues/{issueId}/assign")]
    public static async Task<IResult> Assign(AssignIssue command, IDocumentSession session)
    {
        var user = await session.Query<User>().FirstOrDefaultAsync(u => u.Id == command.AssigneeId);
        if (user is null)
            return Results.NotFound();

        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        var aggregate = stream.Aggregate!;

        if (aggregate.AssigneeId.HasValue)
        {
            stream.AppendOne(new IssueUnassigned(aggregate.Id, aggregate.AssigneeId.Value));
        }

        stream.AppendOne(new IssueAssigned(aggregate.Id, command.AssigneeId, user.Name, aggregate.Title));

        return Results.NoContent();
    }
}
