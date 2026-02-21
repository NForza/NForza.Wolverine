using Alba;
using Shouldly;
using WolverineGettingStarted.Users;

namespace WolverineGettingStarted.Tests.Users;

public class UserEndpointTests : IntegrationContext
{
    public UserEndpointTests(AppFixture fixture) : base(fixture) { }

    [Fact]
    public async Task should_create_a_user()
    {
        var command = new CreateUser("John Doe", "john@example.com");

        var result = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/api/users");
            x.StatusCodeShouldBe(200);
        });

        var user = result.ReadAsJson<User>();
        user.ShouldNotBeNull();
        user.Id.ShouldNotBe(default);
        user.Name.ShouldBe("John Doe");
        user.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public async Task should_get_a_user()
    {
        // Arrange: create a user first
        var command = new CreateUser("Jane Doe", "jane@example.com");

        var createResult = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/api/users");
        });

        var created = createResult.ReadAsJson<User>()!;

        // Act: retrieve the user
        var getResult = await Scenario(x =>
        {
            x.Get.Url($"/api/users/{created.Id}");
            x.StatusCodeShouldBe(200);
        });

        // Assert
        var user = getResult.ReadAsJson<User>();
        user.ShouldNotBeNull();
        user.Name.ShouldBe("Jane Doe");
        user.Email.ShouldBe("jane@example.com");
    }
}
