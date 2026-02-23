using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Reports;

public class AssigneeIssueReport
{
    public UserId Id { get; set; }
    public List<AssigneeIssueItem> Issues { get; set; } = [];
}
