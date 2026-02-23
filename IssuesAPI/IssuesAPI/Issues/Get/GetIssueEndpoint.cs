using Marten;
using Wolverine.Http;
using Wolverine.Issues.Issues.Model;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Issues.Issues.Get;

public static class GetIssueEndpoint
{
    [WolverineGet("/issues/{id}")]
    public static Task<Issue?> GetIssue(IssueId id, IQuerySession session) =>
        session.Events.AggregateStreamAsync<Issue>(id);
}
