using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Contracts.Issues.Lifecycle;

public record IssueOpened(IssueId IssueId, UserId AssigneeId, DateTimeOffset Reopened);
