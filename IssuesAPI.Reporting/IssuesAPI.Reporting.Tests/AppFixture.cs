using Alba;
using Testcontainers.PostgreSql;

namespace Wolverine.Reporting.Tests;

public class AppFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("reporting")
        .Build();

    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Host = await AlbaHost.For<Program>(x =>
        {
            x.ConfigureServices((_, services) =>
            {
                services.DisableAllExternalWolverineTransports();
            });
            x.UseSetting("ConnectionStrings:Reporting", _postgres.GetConnectionString());
        });
    }

    public async Task DisposeAsync()
    {
        await Host.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
