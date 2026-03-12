using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.Issues.Users;
using Wolverine.Tracking;
using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Tests;

[Collection("integration")]
public abstract class IntegrationContext(AppFixture fixture) : IAsyncLifetime
{
    public IAlbaHost Host => fixture.Host;

    public IDocumentStore Store
        => Host.Services.GetRequiredService<IDocumentStore>();

    public async Task InitializeAsync()
        => await Store.Advanced.ResetAllData();

    public Task DisposeAsync()
        => Task.CompletedTask;

    protected async Task<User> CreateUser(string name, string email)
    {
        var result = await Scenario(x =>
        {
            x.Post.Json(new CreateUser(name, email)).ToUrl("/users");
            x.StatusCodeShouldBe(200);
        });

        return result.ReadAsJson<User>()!;
    }

    protected async Task<IScenarioResult> Scenario(Action<Scenario> configure)
        => await Host.Scenario(configure);

    protected async Task<(ITrackedSession, IScenarioResult)> TrackedHttpCall(Action<Scenario> configure)
    {
        IScenarioResult result = null!;
        var tracked = await Host.ExecuteAndWaitAsync(async () => result = await Host.Scenario(configure));
        return (tracked, result);
    }
}
