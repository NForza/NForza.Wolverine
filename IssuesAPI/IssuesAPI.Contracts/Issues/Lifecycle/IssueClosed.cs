using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Contracts.Issues.Lifecycle;

public record IssueClosed(IssueId IssueId, UserId AssigneeId, DateTimeOffset Closed);
