using Marten;
using Wolverine.Http;
using Wolverine.Issues.Issues.Model;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Issues.Issues.Get;

public static class GetIssueEndpoint
{
    [WolverineGet("/api/issues/{id}")]
    public static async Task<Issue?> GetIssue(IssueId id, IQuerySession session)
    {
        return await session.Events.AggregateStreamAsync<Issue>(id);
    }
}
