using Marten;
using Wolverine.Http;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Reporting.Reports;

public static class IssueReportEndpoint
{
    [WolverineGet("/api/reports/issues")]
    public static Task<IReadOnlyList<IssueReport>> GetAll(IQuerySession session)
    {
        return session.Query<IssueReport>().ToListAsync();
    }

    [WolverineGet("/api/reports/issues/{id}")]
    public static Task<IssueReport?> Get(IssueId id, IQuerySession session)
    {
        return session.Query<IssueReport>().Where(r => r.Id == id).FirstOrDefaultAsync();
    }
}
