using Marten;
using Wolverine.Http;
using Wolverine.Marten;

namespace WolverineGettingStarted.Issues;

public record IssueCreatedResponse(IssueId Id, string Title, string Description);

public static class IssueEndpoints
{
    [WolverinePost("/api/issues")]
    public static (IssueCreatedResponse, IStartStream) Post(CreateIssue command)
    {
        var created = new IssueCreated(
            new IssueId(),
            command.OriginatorId,
            command.Title,
            command.Description,
            DateTimeOffset.UtcNow
        );

        var startStream = MartenOps.StartStream<Issue>(created.Id, created);

        var response = new IssueCreatedResponse(created.Id, created.Title, created.Description);

        return (response, startStream);
    }

    [WolverineGet("/api/issues/{id}")]
    public static async Task<Issue?> GetIssue(IssueId id, IQuerySession session)
    {
        return await session.Events.AggregateStreamAsync<Issue>(id);
    }

    [EmptyResponse]
    [WolverinePut("/api/issues/{issueId}/assign")]
    public static async Task Assign(AssignIssue command, IDocumentSession session)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        stream.AppendOne(new IssueAssigned(stream.Aggregate!.Id, command.AssigneeId));
    }

    [EmptyResponse]
    [WolverinePut("/api/issues/{issueId}/close")]
    public static async Task Close(CloseIssue command, IDocumentSession session)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        stream.AppendOne(new IssueClosed(stream.Aggregate!.Id, DateTimeOffset.UtcNow));
    }

    [EmptyResponse]
    [WolverinePut("/api/issues/{issueId}/reopen")]
    public static async Task Reopen(ReopenIssue command, IDocumentSession session)
    {
        var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
        stream.AppendOne(new IssueOpened(stream.Aggregate!.Id, DateTimeOffset.UtcNow));
    }

    [WolverineGet("/api/issues/{id}/summary")]
    public static Task<IssueSummary?> GetSummary(IssueId id, IQuerySession session)
    {
        return session.Query<IssueSummary>().Where(s => s.Id == id).FirstOrDefaultAsync();
    }
}
