using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Issues.Creation;

public record CreateIssue(UserId OriginatorId, string Title, string Description);
