using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Reporting.Reports;

public class AssigneeIssueItem
{
    public IssueId IssueId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
}
