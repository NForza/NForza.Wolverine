using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Issues.Assignment;

public record AssignIssue(IssueId IssueId, UserId AssigneeId);
