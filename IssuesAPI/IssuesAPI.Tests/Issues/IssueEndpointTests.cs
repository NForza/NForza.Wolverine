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

public class IssueEndpointTests(AppFixture fixture) : IntegrationContext(fixture)
{
    [Fact]
    public async Task should_create_an_issue()
    {
        var originator = await CreateUser("Alice", "alice@example.com");

        var command = new CreateIssue(
            originator.Id,
            "Login page not loading",
            "Users cannot access the login page"
        );

        var (tracked, result) = await TrackedHttpCall(x =>
        {
            x.Post.Json(command).ToUrl("/issues");
            x.StatusCodeShouldBe(200);
        });

        var response = result.ReadAsJson<IssueCreatedResponse>();
        response.ShouldNotBeNull();
        response.Id.ShouldNotBe(default);
        response.Title.ShouldBe("Login page not loading");
        response.Description.ShouldBe("Users cannot access the login page");
    }

    [Fact]
    public async Task should_fail_to_create_issue_with_nonexistent_user()
    {
        await Scenario(x =>
        {
            x.Post.Json(new CreateIssue(new UserId(), "Title", "Description"))
                .ToUrl("/issues");
            x.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task should_fail_to_assign_issue_to_nonexistent_user()
    {
        var originator = await CreateUser("Ivan", "ivan@example.com");

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(new CreateIssue(originator.Id, "Assign fail test", "Desc"))
                .ToUrl("/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        await Scenario(x =>
        {
            x.Put.Json(new AssignIssue(created.Id, new UserId()))
                .ToUrl($"/issues/{created.Id}/assign");
            x.StatusCodeShouldBe(404);
        });
    }

    [Fact]
    public async Task should_get_an_issue()
    {
        var originator = await CreateUser("Bob", "bob@example.com");

        var command = new CreateIssue(
            originator.Id,
            "Payment button broken",
            "Users cannot submit payments"
        );

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(command).ToUrl("/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        await using var session = Store.QuerySession();
        var stored = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        stored.ShouldNotBeNull("Issue should exist in Marten after TrackedHttpCall");

        var getResult = await Scenario(x =>
        {
            x.Get.Url($"/issues/{created.Id}");
            x.StatusCodeShouldBe(200);
        });

        var issue = getResult.ReadAsJson<Issue>();
        issue.ShouldNotBeNull();
        issue.Title.ShouldBe("Payment button broken");
        issue.Description.ShouldBe("Users cannot submit payments");
        issue.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task should_assign_an_issue()
    {
        var originator = await CreateUser("Charlie", "charlie@example.com");
        var assignee = await CreateUser("Diana", "diana@example.com");

        var createCommand = new CreateIssue(
            originator.Id,
            "Database migration needed",
            "Old tables need cleanup"
        );

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(createCommand).ToUrl("/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        var assignCommand = new AssignIssue(created.Id, assignee.Id);

        await TrackedHttpCall(x =>
        {
            x.Put.Json(assignCommand).ToUrl($"/issues/{created.Id}/assign");
            x.StatusCodeShouldBe(204);
        });

        await using var session = Store.QuerySession();
        var issue = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        issue.ShouldNotBeNull();
        issue.AssigneeId.ShouldBe(assignee.Id);
    }

    [Fact]
    public async Task should_store_full_event_stream()
    {
        var originator = await CreateUser("Eve", "eve@example.com");
        var assignee = await CreateUser("Frank", "frank@example.com");

        var createCommand = new CreateIssue(originator.Id, "Event stream test", "Verify events are stored");

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(createCommand).ToUrl("/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        await TrackedHttpCall(x =>
        {
            x.Put.Json(new AssignIssue(created.Id, assignee.Id))
                .ToUrl($"/issues/{created.Id}/assign");
            x.StatusCodeShouldBe(204);
        });

        await using var session = Store.QuerySession();
        var events = await session.Events.FetchStreamAsync(created.Id);

        events.Count.ShouldBe(2);

        var createdEvent = events[0].Data.ShouldBeOfType<IssueCreated>();
        createdEvent.Title.ShouldBe("Event stream test");
        createdEvent.OriginatorId.ShouldBe(originator.Id);
        createdEvent.OriginatorName.ShouldBe("Eve");

        var assignedEvent = events[1].Data.ShouldBeOfType<IssueAssigned>();
        assignedEvent.AssigneeId.ShouldBe(assignee.Id);
        assignedEvent.AssigneeName.ShouldBe("Frank");

        events[0].Version.ShouldBe(1);
        events[1].Version.ShouldBe(2);

        var snapshot = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        snapshot.ShouldNotBeNull();
        snapshot.Title.ShouldBe("Event stream test");
        snapshot.AssigneeId.ShouldBe(assignee.Id);
        snapshot.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public async Task should_close_an_issue()
    {
        var originator = await CreateUser("Grace", "grace@example.com");

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(new CreateIssue(originator.Id, "Close me", "Will be closed"))
                .ToUrl("/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        await TrackedHttpCall(x =>
        {
            x.Put.Json(new CloseIssue(created.Id))
                .ToUrl($"/issues/{created.Id}/close");
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
        var originator = await CreateUser("Heidi", "heidi@example.com");

        var (_, createResult) = await TrackedHttpCall(x =>
        {
            x.Post.Json(new CreateIssue(originator.Id, "Reopen me", "Will be reopened"))
                .ToUrl("/issues");
        });

        var created = createResult.ReadAsJson<IssueCreatedResponse>()!;

        await TrackedHttpCall(x =>
        {
            x.Put.Json(new CloseIssue(created.Id))
                .ToUrl($"/issues/{created.Id}/close");
            x.StatusCodeShouldBe(204);
        });

        await TrackedHttpCall(x =>
        {
            x.Put.Json(new ReopenIssue(created.Id))
                .ToUrl($"/issues/{created.Id}/reopen");
            x.StatusCodeShouldBe(204);
        });

        await using var session = Store.QuerySession();
        var issue = await session.Events.AggregateStreamAsync<Issue>(created.Id);
        issue.ShouldNotBeNull();
        issue.IsOpen.ShouldBeTrue();

        var events = await session.Events.FetchStreamAsync(created.Id);
        events.Count.ShouldBe(3);
        events[0].Data.ShouldBeOfType<IssueCreated>();
        events[1].Data.ShouldBeOfType<IssueClosed>();
        events[2].Data.ShouldBeOfType<IssueOpened>();
    }
}
