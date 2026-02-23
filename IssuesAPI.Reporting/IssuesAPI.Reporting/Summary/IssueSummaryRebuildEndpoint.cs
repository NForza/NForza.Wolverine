using Marten;
using Wolverine.Http;

namespace Wolverine.Reporting.Summary;

public static class IssueSummaryRebuildEndpoint
{
    [EmptyResponse]
    [WolverinePost("/admin/projections/issue-summary/rebuild")]
    public static async Task Rebuild(IDocumentStore store)
    {
        using var daemon = await store.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync<IssueSummaryProjection>(CancellationToken.None);
    }
}
