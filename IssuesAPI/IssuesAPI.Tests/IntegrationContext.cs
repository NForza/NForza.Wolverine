using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.Tracking;

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

    protected async Task<IScenarioResult> Scenario(Action<Scenario> configure)
        => await Host.Scenario(configure);

    protected async Task<(ITrackedSession, IScenarioResult)> TrackedHttpCall(Action<Scenario> configure)
    {
        IScenarioResult result = null!;
        var tracked = await Host.ExecuteAndWaitAsync(async () => result = await Host.Scenario(configure));
        return (tracked, result);
    }
}
