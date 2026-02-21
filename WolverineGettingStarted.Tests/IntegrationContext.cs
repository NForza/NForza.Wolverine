using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.Tracking;

namespace WolverineGettingStarted.Tests;

[Collection("integration")]
public abstract class IntegrationContext : IAsyncLifetime
{
    private readonly AppFixture _fixture;

    protected IntegrationContext(AppFixture fixture)
    {
        _fixture = fixture;
    }

    public IAlbaHost Host => _fixture.Host;

    public IDocumentStore Store =>
        Host.Services.GetRequiredService<IDocumentStore>();

    public async Task InitializeAsync()
    {
        await Store.Advanced.ResetAllData();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<IScenarioResult> Scenario(Action<Scenario> configure)
    {
        return await Host.Scenario(configure);
    }

    protected async Task<(ITrackedSession, IScenarioResult)> TrackedHttpCall(
        Action<Scenario> configure)
    {
        IScenarioResult result = null!;
        var tracked = await Host.ExecuteAndWaitAsync(async () =>
        {
            result = await Host.Scenario(configure);
        });
        return (tracked, result);
    }
}
