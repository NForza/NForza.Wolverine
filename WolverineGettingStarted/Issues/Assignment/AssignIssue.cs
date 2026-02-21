using WolverineGettingStarted.Users;

namespace WolverineGettingStarted.Issues;

public record AssignIssue(IssueId IssueId, UserId AssigneeId);
