using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Hubs;
using Wolverine.Issues.Issues.Model;
using Wolverine.Marten;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Issues.Issues.Creation;

public record IssueCreatedResponse(IssueId Id, string Title, string Description);

public static class CreateIssueEndpoint
{
    [WolverinePost("/issues")]
    public static (IssueCreatedResponse, IStartStream) Post(CreateIssue command, IssueEventBroadcaster broadcaster)
    {
        var created = new IssueCreated(
            new IssueId(),
            command.OriginatorId,
            command.Title,
            command.Description,
            DateTimeOffset.UtcNow
        );

        var startStream = MartenOps.StartStream<Issue>(created.Id.AsGuid(), created);

        _ = broadcaster.Broadcast("IssueCreated", created);

        var response = new IssueCreatedResponse(created.Id, created.Title, created.Description);

        return (response, startStream);
    }
}
