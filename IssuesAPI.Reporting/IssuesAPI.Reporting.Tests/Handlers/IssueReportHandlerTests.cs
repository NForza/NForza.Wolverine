using Marten;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using Wolverine.Reporting.Reports;
using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Tests.Handlers;

public class IssueReportProjectionTests : IntegrationContext
{
    public IssueReportProjectionTests(AppFixture fixture) : base(fixture) { }

    private async Task AppendEvents(IssueId issueId, params object[] events)
    {
        await using var session = Host.Services.GetRequiredService<IDocumentStore>().LightweightSession();
        session.Events.StartStream<AssigneeIssueReport>(issueId.AsGuid(), events);
        await session.SaveChangesAsync();
    }

    private async Task AppendToStream(IssueId issueId, params object[] events)
    {
        await using var session = Host.Services.GetRequiredService<IDocumentStore>().LightweightSession();
        session.Events.Append(issueId.AsGuid(), events);
        await session.SaveChangesAsync();
    }

    [Fact]
    public async Task should_create_report_when_issue_assigned()
    {
        var issueId = new IssueId();
        var assigneeId = new UserId();

        await AppendEvents(issueId,
            new IssueCreated(issueId, new UserId(), "Test Issue", "Test Description", DateTimeOffset.UtcNow));
        await AppendToStream(issueId,
            new IssueAssigned(issueId, assigneeId, "Test Issue"));

        await using var session = Store.QuerySession();
        var report = await session.Query<AssigneeIssueReport>()
            .FirstOrDefaultAsync(r => r.Id == assigneeId);

        report.ShouldNotBeNull();
        report.Issues.Count.ShouldBe(1);
        report.Issues[0].IssueId.ShouldBe(issueId);
        report.Issues[0].Title.ShouldBe("Test Issue");
        report.Issues[0].Status.ShouldBe("Open");
    }

    [Fact]
    public async Task should_close_issue_in_report()
    {
        var issueId = new IssueId();
        var assigneeId = new UserId();

        await AppendEvents(issueId,
            new IssueCreated(issueId, new UserId(), "Close test", "Desc", DateTimeOffset.UtcNow));
        await AppendToStream(issueId,
            new IssueAssigned(issueId, assigneeId, "Close test"));
        await AppendToStream(issueId,
            new IssueClosed(issueId, assigneeId, DateTimeOffset.UtcNow));

        await using var session = Store.QuerySession();
        var report = await session.Query<AssigneeIssueReport>()
            .FirstOrDefaultAsync(r => r.Id == assigneeId);

        report.ShouldNotBeNull();
        report.Issues[0].Status.ShouldBe("Closed");
    }

    [Fact]
    public async Task should_reopen_issue_in_report()
    {
        var issueId = new IssueId();
        var assigneeId = new UserId();

        await AppendEvents(issueId,
            new IssueCreated(issueId, new UserId(), "Reopen test", "Desc", DateTimeOffset.UtcNow));
        await AppendToStream(issueId,
            new IssueAssigned(issueId, assigneeId, "Reopen test"));
        await AppendToStream(issueId,
            new IssueClosed(issueId, assigneeId, DateTimeOffset.UtcNow.AddMinutes(-1)));
        await AppendToStream(issueId,
            new IssueOpened(issueId, assigneeId, DateTimeOffset.UtcNow));

        await using var session = Store.QuerySession();
        var report = await session.Query<AssigneeIssueReport>()
            .FirstOrDefaultAsync(r => r.Id == assigneeId);

        report.ShouldNotBeNull();
        report.Issues[0].Status.ShouldBe("Open");
    }

    [Fact]
    public async Task should_track_multiple_issues_for_same_assignee()
    {
        var issueId1 = new IssueId();
        var issueId2 = new IssueId();
        var assigneeId = new UserId();

        await AppendEvents(issueId1,
            new IssueCreated(issueId1, new UserId(), "Issue 1", "Desc", DateTimeOffset.UtcNow));
        await AppendToStream(issueId1,
            new IssueAssigned(issueId1, assigneeId, "Issue 1"));

        await AppendEvents(issueId2,
            new IssueCreated(issueId2, new UserId(), "Issue 2", "Desc", DateTimeOffset.UtcNow));
        await AppendToStream(issueId2,
            new IssueAssigned(issueId2, assigneeId, "Issue 2"));

        await using var session = Store.QuerySession();
        var report = await session.Query<AssigneeIssueReport>()
            .FirstOrDefaultAsync(r => r.Id == assigneeId);

        report.ShouldNotBeNull();
        report.Issues.Count.ShouldBe(2);
        report.Issues.ShouldContain(i => i.IssueId == issueId1);
        report.Issues.ShouldContain(i => i.IssueId == issueId2);
    }

    [Fact]
    public async Task should_move_issue_to_new_assignee_on_reassignment()
    {
        var issueId = new IssueId();
        var assigneeA = new UserId();
        var assigneeB = new UserId();

        await AppendEvents(issueId,
            new IssueCreated(issueId, new UserId(), "Reassign test", "Desc", DateTimeOffset.UtcNow));
        await AppendToStream(issueId,
            new IssueAssigned(issueId, assigneeA, "Reassign test"));
        await AppendToStream(issueId,
            new IssueUnassigned(issueId, assigneeA));
        await AppendToStream(issueId,
            new IssueAssigned(issueId, assigneeB, "Reassign test"));

        await using var session = Store.QuerySession();

        var reportA = await session.Query<AssigneeIssueReport>()
            .FirstOrDefaultAsync(r => r.Id == assigneeA);
        reportA.ShouldNotBeNull();
        reportA.Issues.Count.ShouldBe(0);

        var reportB = await session.Query<AssigneeIssueReport>()
            .FirstOrDefaultAsync(r => r.Id == assigneeB);
        reportB.ShouldNotBeNull();
        reportB.Issues.Count.ShouldBe(1);
        reportB.Issues[0].IssueId.ShouldBe(issueId);
        reportB.Issues[0].Title.ShouldBe("Reassign test");
    }

    [Fact]
    public async Task should_track_full_lifecycle()
    {
        var issueId = new IssueId();
        var assigneeId = new UserId();
        var created = DateTimeOffset.UtcNow;

        await AppendEvents(issueId,
            new IssueCreated(issueId, new UserId(), "Lifecycle test", "Full lifecycle", created));
        await AppendToStream(issueId,
            new IssueAssigned(issueId, assigneeId, "Lifecycle test"));
        await AppendToStream(issueId,
            new IssueClosed(issueId, assigneeId, created.AddHours(1)));
        await AppendToStream(issueId,
            new IssueOpened(issueId, assigneeId, created.AddHours(2)));

        await using var session = Store.QuerySession();
        var report = await session.Query<AssigneeIssueReport>()
            .FirstOrDefaultAsync(r => r.Id == assigneeId);

        report.ShouldNotBeNull();
        report.Issues.Count.ShouldBe(1);
        report.Issues[0].Title.ShouldBe("Lifecycle test");
        report.Issues[0].Status.ShouldBe("Open");
    }
}
