using Marten;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Issues.Model;
using Wolverine.Issues.Users;
using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Issues.Issues.Creation;

public record IssueCreatedResponse(IssueId Id, string Title, string Description);

public static class CreateIssueEndpoint
{
    [WolverinePost("/issues")]
    public static async Task<IResult> Post(CreateIssue command, IDocumentSession session)
    {
        var user = await session.Query<User>().FirstOrDefaultAsync(u => u.Id == command.OriginatorId);
        if (user is null)
            return Results.NotFound();

        var created = new IssueCreated(
            new IssueId(),
            command.OriginatorId,
            user.Name,
            command.Title,
            command.Description,
            DateTimeOffset.UtcNow
        );

        session.Events.StartStream<Issue>(created.Id.AsGuid(), created);

        return Results.Ok(new IssueCreatedResponse(created.Id, created.Title, created.Description));
    }
}
