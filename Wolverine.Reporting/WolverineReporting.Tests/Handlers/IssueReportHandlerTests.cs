using Marten;
using Shouldly;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using Wolverine.Reporting.Reports;
using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Tests.Handlers;

public class IssueReportHandlerTests : IntegrationContext
{
    public IssueReportHandlerTests(AppFixture fixture) : base(fixture) { }

    [Fact]
    public async Task should_create_report_on_issue_created()
    {
        var issueId = new IssueId();
        var originatorId = new UserId();

        await SendMessage(new IssueCreated(
            issueId, originatorId, "Test Issue", "Test Description", DateTimeOffset.UtcNow));

        await using var session = Store.QuerySession();
        var report = await session.Query<IssueReport>()
            .Where(r => r.Id == issueId)
            .FirstOrDefaultAsync();

        report.ShouldNotBeNull();
        report.Title.ShouldBe("Test Issue");
        report.Description.ShouldBe("Test Description");
        report.OriginatorId.ShouldBe(originatorId);
        report.Status.ShouldBe("Open");
        report.EventCount.ShouldBe(1);
    }

    [Fact]
    public async Task should_update_assignee_on_issue_assigned()
    {
        var issueId = new IssueId();
        var assigneeId = new UserId();

        await SendMessage(new IssueCreated(
            issueId, new UserId(), "Assign test", "Desc", DateTimeOffset.UtcNow));
        await SendMessage(new IssueAssigned(issueId, assigneeId));

        await using var session = Store.QuerySession();
        var report = await session.Query<IssueReport>()
            .Where(r => r.Id == issueId)
            .FirstOrDefaultAsync();

        report.ShouldNotBeNull();
        report.AssigneeId.ShouldBe(assigneeId);
        report.EventCount.ShouldBe(2);
    }

    [Fact]
    public async Task should_close_report()
    {
        var issueId = new IssueId();
        var closed = DateTimeOffset.UtcNow;

        await SendMessage(new IssueCreated(
            issueId, new UserId(), "Close test", "Desc", DateTimeOffset.UtcNow));
        await SendMessage(new IssueClosed(issueId, closed));

        await using var session = Store.QuerySession();
        var report = await session.Query<IssueReport>()
            .Where(r => r.Id == issueId)
            .FirstOrDefaultAsync();

        report.ShouldNotBeNull();
        report.Status.ShouldBe("Closed");
        report.Closed.ShouldBe(closed);
        report.EventCount.ShouldBe(2);
    }

    [Fact]
    public async Task should_reopen_report()
    {
        var issueId = new IssueId();
        var reopened = DateTimeOffset.UtcNow;

        await SendMessage(new IssueCreated(
            issueId, new UserId(), "Reopen test", "Desc", DateTimeOffset.UtcNow));
        await SendMessage(new IssueClosed(issueId, DateTimeOffset.UtcNow.AddMinutes(-1)));
        await SendMessage(new IssueOpened(issueId, reopened));

        await using var session = Store.QuerySession();
        var report = await session.Query<IssueReport>()
            .Where(r => r.Id == issueId)
            .FirstOrDefaultAsync();

        report.ShouldNotBeNull();
        report.Status.ShouldBe("Open");
        report.Closed.ShouldBeNull();
        report.EventCount.ShouldBe(3);
    }

    [Fact]
    public async Task should_track_full_lifecycle()
    {
        var issueId = new IssueId();
        var originatorId = new UserId();
        var assigneeId = new UserId();
        var created = DateTimeOffset.UtcNow;

        await SendMessage(new IssueCreated(
            issueId, originatorId, "Lifecycle test", "Full lifecycle", created));
        await SendMessage(new IssueAssigned(issueId, assigneeId));
        await SendMessage(new IssueClosed(issueId, created.AddHours(1)));
        await SendMessage(new IssueOpened(issueId, created.AddHours(2)));

        await using var session = Store.QuerySession();
        var report = await session.Query<IssueReport>()
            .Where(r => r.Id == issueId)
            .FirstOrDefaultAsync();

        report.ShouldNotBeNull();
        report.Title.ShouldBe("Lifecycle test");
        report.OriginatorId.ShouldBe(originatorId);
        report.AssigneeId.ShouldBe(assigneeId);
        report.Status.ShouldBe("Open");
        report.Closed.ShouldBeNull();
        report.EventCount.ShouldBe(4);
    }
}
