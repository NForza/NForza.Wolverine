using Alba;

namespace Wolverine.Reporting.Tests;

public class AppFixture : IAsyncLifetime
{
    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Host = await AlbaHost.For<Program>(x =>
        {
            x.ConfigureServices((_, services) =>
            {
                services.DisableAllExternalWolverineTransports();
            });
        });
    }

    public async Task DisposeAsync() => await Host.DisposeAsync();
}
