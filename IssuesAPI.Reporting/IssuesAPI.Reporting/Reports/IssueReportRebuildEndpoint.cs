using Marten;
using Wolverine.Http;

namespace Wolverine.Reporting.Reports;

public static class IssueReportRebuildEndpoint
{
    [EmptyResponse]
    [WolverinePost("/admin/projections/issue-report/rebuild")]
    public static async Task Rebuild(IDocumentStore store)
    {
        using var daemon = await store.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync<IssueReportProjection>(CancellationToken.None);
    }
}
