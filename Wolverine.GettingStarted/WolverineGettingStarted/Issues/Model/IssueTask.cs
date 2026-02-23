using WolverineGettingStarted.Issues.Model;

namespace Wolverine.Issues.Issues.Model;

public class IssueTask
{
    public IssueTask(string title, string description)
    {
        Title = title;
        Description = description;
        Id = new IssueTaskId();
    }

    public IssueTaskId Id { get; set; }

    public string Title { get; set; }
    public string Description { get; set; }
    public DateTimeOffset? Started { get; set; }
    public DateTimeOffset Finished { get; set; }
}
