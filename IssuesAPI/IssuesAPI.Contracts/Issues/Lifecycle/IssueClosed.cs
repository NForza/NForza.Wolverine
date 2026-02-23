using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Issues.Contracts.Issues.Lifecycle;

public record IssueClosed(IssueId IssueId, DateTimeOffset Closed);
