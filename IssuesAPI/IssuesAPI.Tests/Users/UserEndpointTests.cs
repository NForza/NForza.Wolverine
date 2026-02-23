using Shouldly;
using Wolverine.Issues.Users;

namespace Wolverine.Issues.Tests.Users;

public class UserEndpointTests(AppFixture fixture) : IntegrationContext(fixture)
{
    [Fact]
    public async Task should_create_a_user()
    {
        var command = new CreateUser("John Doe", "john@example.com");

        var result = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/users");
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
        var command = new CreateUser("Jane Doe", "jane@example.com");

        var createResult = await Scenario(x => x.Post.Json(command).ToUrl("/users"));

        var created = createResult.ReadAsJson<User>()!;

        var getResult = await Scenario(x =>
        {
            x.Get.Url($"/users/{created.Id}");
            x.StatusCodeShouldBe(200);
        });

        var user = getResult.ReadAsJson<User>();
        user.ShouldNotBeNull();
        user.Name.ShouldBe("Jane Doe");
        user.Email.ShouldBe("jane@example.com");
    }
}
