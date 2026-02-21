using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;
using WolverineGettingStarted.Issues;
using WolverineGettingStarted.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddWolverineHttp();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Marten")!;

builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    opts.Schema.For<User>();
    opts.Projections.Add<IssueSummaryProjection>(ProjectionLifecycle.Inline);
})
.IntegrateWithWolverine()
.UseLightweightSessions();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
    opts.Include<NForza.Wolverine.ValueTypes.WolverineValueTypeExtension>();
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapWolverineEndpoints();
app.UseHttpsRedirection();

app.Run();
