using Alba;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.Tracking;

namespace Wolverine.Reporting.Tests;

[Collection("integration")]
public abstract class IntegrationContext(AppFixture fixture) : IAsyncLifetime
{
    public IAlbaHost Host => fixture.Host;

    public IDocumentStore Store => Host.Services.GetRequiredService<IDocumentStore>();

    public async Task InitializeAsync() => await Store.Advanced.ResetAllData();

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<IScenarioResult> Scenario(Action<Scenario> configure) => await Host.Scenario(configure);

    protected async Task<ITrackedSession> SendMessage(object message) => await Host.InvokeMessageAndWaitAsync(message);
}
