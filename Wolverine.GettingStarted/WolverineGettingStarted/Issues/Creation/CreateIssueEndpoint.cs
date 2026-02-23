using Wolverine.Http;
using Wolverine.Marten;
using WolverineGettingStarted.Issues.Model;

namespace WolverineGettingStarted.Issues.Creation;

public record IssueCreatedResponse(IssueId Id, string Title, string Description);

public static class CreateIssueEndpoint
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

        var startStream = MartenOps.StartStream<Issue>(created.Id.AsGuid(), created);

        var response = new IssueCreatedResponse(created.Id, created.Title, created.Description);

        return (response, startStream);
    }
}
