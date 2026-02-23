using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using Wolverine.Issues.Issues.Model;

namespace Wolverine.Issues.Issues.Lifecycle;

public static class CloseIssueEndpoint
{
    [EmptyResponse]
    [WolverinePut("/api/issues/{issueId}/close")]
    public static async Task Close(CloseIssue command, IDocumentSession session)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        stream.AppendOne(new IssueClosed(stream.Aggregate!.Id, DateTimeOffset.UtcNow));
    }
}
