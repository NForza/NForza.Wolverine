using Marten;
using Shouldly;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Contracts.Issues.Lifecycle;
using Wolverine.Issues.Issues.Assignment;
using Wolverine.Issues.Issues.Creation;
using Wolverine.Issues.Issues.Lifecycle;
using Wolverine.Issues.Issues.Model;
using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Tests.Issues;

public class IssueEndpointTests : IntegrationContext
{
    public IssueEndpointTests(AppFixture fixture) : base(fixture) { }

    [Fact]
    public async Task should_create_an_issue()
    {
        var command = new CreateIssue(
            new UserId(),
            "Login page not loading",
            "Users cannot access the login page"
        );

        var (tracked, result) = await TrackedHttpCall(x =>
        {
            x.Post.Json(command).ToUrl("/api/issues");
            x.StatusCodeShouldBe(200);
        });

        var response = result.ReadAsJson<IssueCreatedResponse>();
        response.ShouldNotBeNull();
        response.Id.ShouldNotBe(default);
        response.Title.ShouldBe("Login page not loading");
        response.Description.ShouldBe("Users cannot access the login page");
    }

    [Fact]
    public async Task should_get_an_issue()
    {
        // Arrange: create an issue via the API
        var command = new CreateIssue(
            new UserId(),
            "Payment button broken",
            "Users cannot submit payments"
        );

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(command).ToUrl("/api/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        // Verify the issue exists by replaying the event stream
        await using var session = Store.QuerySession();
        var stored = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        stored.ShouldNotBeNull("Issue should exist in Marten after TrackedHttpCall");

        // Act: retrieve the issue via GET
        var getResult = await Scenario(x =>
        {
            x.Get.Url($"/api/issues/{created.Id}");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var issue = getResult.ReadAsJson<Issue>();
        issue.ShouldNotBeNull();
        issue.Title.ShouldBe("Payment button broken");
        issue.Description.ShouldBe("Users cannot submit payments");
        issue.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task should_assign_an_issue()
    {
        // Arrange: create an issue
        var createCommand = new CreateIssue(
            new UserId(),
            "Database migration needed",
            "Old tables need cleanup"
        );

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(createCommand).ToUrl("/api/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;
        var assigneeId = new UserId();

        // Act: assign the issue
        var assignCommand = new AssignIssue(created.Id, assigneeId);

        await TrackedHttpCall(x =>
        {
            x.Put.Json(assignCommand).ToUrl($"/api/issues/{created.Id}/assign");
            x.StatusCodeShouldBe(204);
        });

        // Assert: verify the assignee was set by replaying the event stream
        await using var session = Store.QuerySession();
        var issue = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        issue.ShouldNotBeNull();
        issue.AssigneeId.ShouldBe(assigneeId);
    }

    [Fact]
    public async Task should_store_full_event_stream()
    {
        // Arrange: create an issue then assign it
        var originatorId = new UserId();
        var createCommand = new CreateIssue(originatorId, "Event stream test", "Verify events are stored");

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(createCommand).ToUrl("/api/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;
        var assigneeId = new UserId();

        await TrackedHttpCall(x =>
        {
            x.Put.Json(new AssignIssue(created.Id, assigneeId))
                .ToUrl($"/api/issues/{created.Id}/assign");
            x.StatusCodeShouldBe(204);
        });

        // Act: read the raw event stream from Marten
        await using var session = Store.QuerySession();
        var events = await session.Events.FetchStreamAsync(created.Id);

        // Assert: the stream should have 2 events in order
        events.Count.ShouldBe(2);

        var createdEvent = events[0].Data.ShouldBeOfType<IssueCreated>();
        createdEvent.Title.ShouldBe("Event stream test");
        createdEvent.OriginatorId.ShouldBe(originatorId);

        var assignedEvent = events[1].Data.ShouldBeOfType<IssueAssigned>();
        assignedEvent.AssigneeId.ShouldBe(assigneeId);

        // Verify stream metadata
        events[0].Version.ShouldBe(1);
        events[1].Version.ShouldBe(2);

        // Verify the aggregate matches what we'd get by replaying events
        var snapshot = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        snapshot.ShouldNotBeNull();
        snapshot.Title.ShouldBe("Event stream test");
        snapshot.AssigneeId.ShouldBe(assigneeId);
        snapshot.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task should_close_an_issue()
    {
        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(new CreateIssue(new UserId(), "Close me", "Will be closed"))
                .ToUrl("/api/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        await TrackedHttpCall(x =>
        {
            x.Put.Json(new CloseIssue(created.Id))
                .ToUrl($"/api/issues/{created.Id}/close");
            x.StatusCodeShouldBe(204);
        });

        await using var session = Store.QuerySession();
        var issue = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        issue.ShouldNotBeNull();
        issue.IsOpen.ShouldBeFalse();
    }

    [Fact]
    public async Task should_reopen_a_closed_issue()
    {
        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(new CreateIssue(new UserId(), "Reopen me", "Will be reopened"))
                .ToUrl("/api/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        // Close it first
        await TrackedHttpCall(x =>
        {
            x.Put.Json(new CloseIssue(created.Id))
                .ToUrl($"/api/issues/{created.Id}/close");
            x.StatusCodeShouldBe(204);
        });

        // Reopen it
        await TrackedHttpCall(x =>
        {
            x.Put.Json(new ReopenIssue(created.Id))
                .ToUrl($"/api/issues/{created.Id}/reopen");
            x.StatusCodeShouldBe(204);
        });

        await using var session = Store.QuerySession();
        var issue = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        issue.ShouldNotBeNull();
        issue.IsOpen.ShouldBeTrue();

        // Verify the full event stream has 3 events
        var events = await session.Events.FetchStreamAsync(created.Id);
        events.Count.ShouldBe(3);
        events[0].Data.ShouldBeOfType<IssueCreated>();
        events[1].Data.ShouldBeOfType<IssueClosed>();
        events[2].Data.ShouldBeOfType<IssueOpened>();
    }

}
