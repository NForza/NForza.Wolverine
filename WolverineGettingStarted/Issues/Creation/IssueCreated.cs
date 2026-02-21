using WolverineGettingStarted.Users;

namespace WolverineGettingStarted.Issues;

public record IssueCreated(
    IssueId Id,
    UserId OriginatorId,
    string Title,
    string Description,
    DateTimeOffset Opened
);
