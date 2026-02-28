using Marten;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Reporting.Reports;
using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Tests.Reports;

public class IssueReportEndpointTests : IntegrationContext
{
    public IssueReportEndpointTests(AppFixture fixture) : base(fixture) { }

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
    public async Task should_get_all_reports()
    {
        var issueId1 = new IssueId();
        var issueId2 = new IssueId();
        var assigneeId1 = new UserId();
        var assigneeId2 = new UserId();

        await AppendEvents(issueId1,
            new IssueCreated(issueId1, new UserId(), "Originator", "Report 1", "Desc 1", DateTimeOffset.UtcNow));
        await AppendToStream(issueId1,
            new IssueAssigned(issueId1, assigneeId1, "Assignee 1", "Report 1"));

        await AppendEvents(issueId2,
            new IssueCreated(issueId2, new UserId(), "Originator", "Report 2", "Desc 2", DateTimeOffset.UtcNow));
        await AppendToStream(issueId2,
            new IssueAssigned(issueId2, assigneeId2, "Assignee 2", "Report 2"));

        var result = await Scenario(x =>
        {
            x.Get.Url("/reports/assignees");
            x.StatusCodeShouldBe(200);
        });

        var reports = result.ReadAsJson<List<AssigneeIssueReport>>();
        reports.ShouldNotBeNull();
        reports.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task should_get_report_by_assignee_id()
    {
        var issueId = new IssueId();
        var assigneeId = new UserId();

        await AppendEvents(issueId,
            new IssueCreated(issueId, new UserId(), "Originator", "Specific report", "Desc", DateTimeOffset.UtcNow));
        await AppendToStream(issueId,
            new IssueAssigned(issueId, assigneeId, "Assignee", "Specific report"));

        var result = await Scenario(x =>
        {
            x.Get.Url($"/reports/assignees/{assigneeId}");
            x.StatusCodeShouldBe(200);
        });

        var report = result.ReadAsJson<AssigneeIssueReport>();
        report.ShouldNotBeNull();
        report.Issues.Count.ShouldBe(1);
        report.Issues[0].Title.ShouldBe("Specific report");
    }
}
