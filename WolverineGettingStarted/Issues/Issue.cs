using WolverineGettingStarted.Users;

namespace WolverineGettingStarted.Issues;

public class Issue
{
    public IssueId Id { get; set; }

    public UserId? AssigneeId { get; set; }
    public UserId? OriginatorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsOpen { get; set; }

    public DateTimeOffset Opened { get; set; }

    public IList<IssueTask> Tasks { get; set; }
        = new List<IssueTask>();

    public void Apply(IssueCreated created)
    {
        Id = created.Id;
        OriginatorId = created.OriginatorId;
        Title = created.Title;
        Description = created.Description;
        IsOpen = true;
        Opened = created.Opened;
    }

    public void Apply(IssueAssigned assigned)
    {
        AssigneeId = assigned.AssigneeId;
    }

    public void Apply(IssueClosed closed)
    {
        IsOpen = false;
    }

    public void Apply(IssueOpened opened)
    {
        IsOpen = true;
    }
}
