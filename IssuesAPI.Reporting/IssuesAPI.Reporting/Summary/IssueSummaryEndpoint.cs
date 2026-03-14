using Marten;
using Wolverine.Http;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Reporting.Summary;

public static class IssueSummaryEndpoint
{
    [WolverineGet("/issues/summaries")]
    public static Task<IReadOnlyList<IssueSummary>> GetAll(IQuerySession session)
        => session.Query<IssueSummary>().ToListAsync();

    [WolverineGet("/issues/{id}/summary")]
    public static Task<IssueSummary?> GetSummary(IssueId id, IQuerySession session)
        => session.Query<IssueSummary>().FirstOrDefaultAsync(s => s.Id == id);
}
