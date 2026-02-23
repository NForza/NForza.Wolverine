using Shouldly;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Reporting.Reports;
using WolverineGettingStarted.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Reporting.Tests.Reports;

public class IssueReportEndpointTests : IntegrationContext
{
    public IssueReportEndpointTests(AppFixture fixture) : base(fixture) { }

    [Fact]
    public async Task should_get_all_reports()
    {
        await SendMessage(new IssueCreated(new IssueId(), new UserId(), "Report 1", "Desc 1", DateTimeOffset.UtcNow));
        await SendMessage(new IssueCreated(new IssueId(), new UserId(), "Report 2", "Desc 2", DateTimeOffset.UtcNow));

        var result = await Scenario(x =>
        {
            x.Get.Url("/reports/issues");
            x.StatusCodeShouldBe(200);
        });

        var reports = result.ReadAsJson<List<IssueReport>>();
        reports.ShouldNotBeNull();
        reports.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task should_get_report_by_id()
    {
        var issueId = new IssueId();

        await SendMessage(new IssueCreated(
            issueId, new UserId(), "Specific report", "Desc", DateTimeOffset.UtcNow));

        var result = await Scenario(x =>
        {
            x.Get.Url($"/reports/issues/{issueId}");
            x.StatusCodeShouldBe(200);
        });

        var report = result.ReadAsJson<IssueReport>();
        report.ShouldNotBeNull();
        report.Title.ShouldBe("Specific report");
    }
}
