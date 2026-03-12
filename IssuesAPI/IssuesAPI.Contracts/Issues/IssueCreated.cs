using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Contracts.Issues;

public record IssueCreated(
    IssueId Id,
    UserId OriginatorId,
    string OriginatorName,
    string Title,
    string Description,
    DateTimeOffset OpenedAt
);
