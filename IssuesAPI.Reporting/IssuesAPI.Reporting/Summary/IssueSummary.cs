using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Summary;

public class IssueSummary
{
    public IssueId Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public UserId? AssigneeId { get; set; }
    public DateTimeOffset Created { get; set; }
    public int EventCount { get; set; }
}
