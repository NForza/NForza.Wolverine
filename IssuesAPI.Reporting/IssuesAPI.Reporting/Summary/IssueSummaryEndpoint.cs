using Marten;
using Wolverine.Http;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Reporting.Summary;

public static class IssueSummaryEndpoint
{
    [WolverineGet("/issues/{id}/summary")]
    public static Task<IssueSummary?> GetSummary(IssueId id, IQuerySession session)
    {
        return session.Query<IssueSummary>().Where(s => s.Id == id).FirstOrDefaultAsync();
    }
}
