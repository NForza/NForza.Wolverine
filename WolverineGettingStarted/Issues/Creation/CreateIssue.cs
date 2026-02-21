using WolverineGettingStarted.Users;

namespace WolverineGettingStarted.Issues;

public record CreateIssue(UserId OriginatorId, string Title, string Description);
