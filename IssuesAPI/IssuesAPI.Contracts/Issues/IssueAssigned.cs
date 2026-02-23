using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Contracts.Issues;

public record IssueAssigned(IssueId IssueId, UserId AssigneeId);
