using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Reports;

public class AssigneeIssueReport
{
    public UserId Id { get; set; }
    public string AssigneeName { get; set; } = string.Empty;
    public List<AssigneeIssueItem> Issues { get; set; } = [];
}
