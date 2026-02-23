using Marten;
using NForza.Wolverine.ValueTypes;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.Http;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Users;
using Wolverine.Marten;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWolverineHttp();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("Issues")!;

builder.Services.AddMarten(opts =>
{
    opts.Connection(connectionString);
    opts.Schema.For<User>();
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

    opts.Publish(x =>
    {
        x.MessagesFromNamespaceContaining<IssueCreated>();
        x.ToRabbitExchange("issue-events");
    });
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapWolverineEndpoints();
app.UseHttpsRedirection();

app.Run();
