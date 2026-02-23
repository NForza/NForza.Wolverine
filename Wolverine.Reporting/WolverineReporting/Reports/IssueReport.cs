using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Reports;

public class IssueReport
{
    public IssueId Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public UserId? OriginatorId { get; set; }
    public UserId? AssigneeId { get; set; }
    public string Status { get; set; } = "Open";
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset? Closed { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public int EventCount { get; set; }
}
