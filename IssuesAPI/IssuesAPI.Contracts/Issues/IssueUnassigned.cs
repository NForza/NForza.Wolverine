using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Contracts.Issues;

public record IssueUnassigned(IssueId IssueId, UserId AssigneeId);
