using Marten;
using Wolverine.Http;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Reports;

public static class IssueReportEndpoint
{
    [WolverineGet("/reports/assignees")]
    public static Task<IReadOnlyList<AssigneeIssueReport>> GetAll(IQuerySession session)
        => session.Query<AssigneeIssueReport>().ToListAsync();

    [WolverineGet("/reports/assignees/{userId}")]
    public static Task<AssigneeIssueReport?> Get(UserId userId, IQuerySession session)
        => session.Query<AssigneeIssueReport>().FirstOrDefaultAsync(r => r.Id == userId);
}
