using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Issues.Contracts.Issues.Lifecycle;

public record IssueOpened(IssueId IssueId, DateTimeOffset Reopened);
