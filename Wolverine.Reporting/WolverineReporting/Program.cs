using JasperFx.Events.Projections;
using Marten;
using NForza.Wolverine.ValueTypes;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.RabbitMQ;
using Wolverine.Reporting.Summary;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWolverineHttp();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Reporting")!;

builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    opts.DatabaseSchemaName = "reporting";
    opts.Projections.Add<IssueSummaryProjection>(ProjectionLifecycle.Inline);
})
.IntegrateWithWolverine()
.UseLightweightSessions();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
    opts.Include<WolverineValueTypeExtension>();

    opts.UseRabbitMq(rabbit =>
    {
        rabbit.HostName = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
    }).AutoProvision();

    opts.ListenToRabbitQueue("issue-events-reporting");
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapWolverineEndpoints();
app.UseHttpsRedirection();

app.Run();
