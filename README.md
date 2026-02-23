# Wolverine Event-Sourced Issues API

A reference implementation demonstrating event sourcing, CQRS, and real-time UI updates using the [Wolverine](https://wolverine.netlify.app/) framework with [Marten](https://martendb.io/) for event storage, [RabbitMQ](https://www.rabbitmq.com/) for messaging, and an [Angular](https://angular.dev/) frontend connected via [SignalR](https://learn.microsoft.com/aspnet/core/signalr/).

## Architecture Overview

```
                         +-----------------+
                         |   Issues.UI     |
                         |  (Angular 21)   |
                         +--------+--------+
                                  |
                           SignalR | WebSocket
                                  |
+-----------------------------+   |   +-----------------------------+
|        IssuesAPI            |<--+   |    IssuesAPI.Reporting      |
|       (port 5035)           |       |       (port 5036)           |
|                             |       |                             |
|  Wolverine HTTP Endpoints   |       |  Wolverine HTTP Endpoints   |
|  Marten Event Store         |       |  Marten Document Store      |
|  SignalR Hub                |       |  Marten Inline Projections  |
|  Transactional Outbox       |       |  RabbitMQ Consumer          |
+-------------+---------------+       +-------------+---------------+
              |                                     |
              |  RabbitMQ (conventional routing)     |
              +------------------------------------>+
              |
      +-------+--------+
      |   PostgreSQL    |
      |  (port 5433)    |
      |                 |
      |  DB: issues     |
      |  DB: reporting  |
      +--------+--------+
```

**IssuesAPI** is the write side: it accepts commands, appends domain events to Marten event streams, and publishes them to RabbitMQ via Wolverine's transactional outbox. A Wolverine handler consumes these events back from RabbitMQ and broadcasts them to the Angular UI over SignalR.

**IssuesAPI.Reporting** is the read side: it consumes events from RabbitMQ, builds denormalized read models, and exposes query endpoints.

**Issues.UI** is an Angular SPA that connects to the IssuesAPI's SignalR hub and displays events in real time.

## Projects

| Project | Description |
|---|---|
| `IssuesAPI/IssuesAPI` | Main API with command endpoints, event sourcing, SignalR hub |
| `IssuesAPI/IssuesAPI.Contracts` | Shared events, commands, and value types. Generates TypeScript via Reinforced.Typings |
| `IssuesAPI/IssuesAPI.Tests` | Integration tests using Alba and Wolverine message tracking |
| `IssuesAPI.Reporting/IssuesAPI.Reporting` | Reporting service with RabbitMQ consumers and query endpoints |
| `IssuesAPI.Reporting/IssuesAPI.Reporting.Tests` | Integration tests for handlers and endpoints |
| `Wolverine.ValueTypes/src` | Attributes and interfaces for strongly-typed value types |
| `Wolverine.ValueTypes/Generators` | C# source generator for value type record structs |
| `Wolverine.ValueTypes/Tests` | Source generator output verification tests |
| `Issues.UI` | Angular 21 SPA with SignalR real-time event display |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20.19+ or 22.12+ (for Angular 21)
- [Docker](https://www.docker.com/) (for PostgreSQL and RabbitMQ)

## Getting Started

### 1. Start Infrastructure

```bash
cd IssuesAPI
docker compose up -d
```

This starts:
- **PostgreSQL** on port 5433 with databases `issues` and `reporting`
- **RabbitMQ** on port 5672 (AMQP) and 15672 (management UI)

### 2. Run the APIs

```bash
# Terminal 1 - IssuesAPI (port 5035)
dotnet run --project IssuesAPI/IssuesAPI

# Terminal 2 - Reporting service (port 5036)
dotnet run --project IssuesAPI.Reporting/IssuesAPI.Reporting
```

API documentation is available at:
- IssuesAPI: http://localhost:5035/scalar/v1
- Reporting: http://localhost:5036/scalar/v1

### 3. Run the Angular UI

```bash
cd Issues.UI
npm install
npx ng serve
```

Open http://localhost:4200. The UI connects to the IssuesAPI SignalR hub and displays events in real time as they occur.

### 4. Run Tests

```bash
dotnet test Wolverine.slnx
```

Runs all 24 tests across 3 test projects (9 value type + 8 IssuesAPI + 7 reporting).

## Domain Model

### Events

Events are the source of truth, stored in Marten event streams:

| Event | Fields | Description |
|---|---|---|
| `IssueCreated` | `Id`, `OriginatorId`, `Title`, `Description`, `OpenedAt` | A new issue was opened |
| `IssueAssigned` | `IssueId`, `AssigneeId` | An issue was assigned to a user |
| `IssueClosed` | `IssueId`, `Closed` | An issue was closed |
| `IssueOpened` | `IssueId`, `Reopened` | A closed issue was reopened |

### Aggregates

The `Issue` aggregate replays events to build current state:

```
IssueCreated  -> sets Id, Title, Description, OriginatorId, IsOpen=true
IssueAssigned -> sets AssigneeId
IssueClosed   -> sets IsOpen=false
IssueOpened   -> sets IsOpen=true
```

### Value Types

Strongly-typed wrappers generated by the source generator eliminate primitive obsession:

| Type | Backing Type | Attribute |
|---|---|---|
| `IssueId` | `Guid` | `[GuidValue]` |
| `IssueTaskId` | `Guid` | `[GuidValue]` |
| `UserId` | `Guid` | `[GuidValue]` |

Each generates: record struct, JSON converter, `TryParse`, comparison operators, and Marten extension methods.

## API Endpoints

### IssuesAPI (port 5035)

| Method | Route | Description |
|---|---|---|
| `POST` | `/issues` | Create a new issue |
| `GET` | `/issues/{id}` | Get an issue by replaying its event stream |
| `PUT` | `/issues/{issueId}/assign` | Assign an issue to a user |
| `PUT` | `/issues/{issueId}/close` | Close an issue |
| `PUT` | `/issues/{issueId}/reopen` | Reopen a closed issue |
| `POST` | `/users` | Create a new user |
| `GET` | `/users/{id}` | Get a user by ID |

### IssuesAPI.Reporting (port 5036)

| Method | Route | Description |
|---|---|---|
| `GET` | `/reports/assignees` | List all assignee issue reports |
| `GET` | `/reports/assignees/{userId}` | Get issues assigned to a specific user |
| `GET` | `/issues/{id}/summary` | Get an issue summary (inline projection) |
| `POST` | `/admin/projections/issue-report/rebuild` | Rebuild the assignee issue report projection |
| `POST` | `/admin/projections/issue-summary/rebuild` | Rebuild the summary projection |

## Key Patterns

### Transactional Outbox

Wolverine's integration with Marten ensures that domain events and outgoing messages are committed in the same PostgreSQL transaction. If the transaction fails, no messages are sent to RabbitMQ:

```csharp
// Program.cs
builder.Services.AddMarten(opts => { ... })
    .IntegrateWithWolverine();       // enables outbox

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions(); // wraps handlers in transactions
    opts.UseRabbitMq(rabbit => { ... })
        .UseConventionalRouting()    // exchange per message type, queue per consumer
        .AutoProvision();
});
```

### Event Sourcing with Marten

Endpoints return Wolverine's `IStartStream` to start new event streams or use `FetchForWriting` to append to existing ones:

```csharp
// Create: starts a new stream
var startStream = MartenOps.StartStream<Issue>(created.Id.AsGuid(), created);

// Mutate: appends to an existing stream
var stream = await session.Events.FetchForWriting<Issue>(command.IssueId);
stream.AppendOne(new IssueClosed(stream.Aggregate!.Id, stream.Aggregate!.AssigneeId ?? default, DateTimeOffset.UtcNow));
```

### CQRS via Separate Services

The write side (IssuesAPI) and read side (Reporting) are separate services with their own databases:
- IssuesAPI writes events to the `issues` database
- Events flow through RabbitMQ to the Reporting service
- Reporting builds denormalized documents in the `reporting` database

### Real-Time Updates via SignalR

Events flow from the Marten outbox through RabbitMQ back into the application, where a Wolverine handler forwards them to SignalR:

```csharp
public class IssueEventSignalRBridge(IHubContext<IssuesHub> hub)
{
    public Task Handle(IssueCreated @event) => Broadcast(nameof(IssueCreated), @event);
    public Task Handle(IssueAssigned @event) => Broadcast(nameof(IssueAssigned), @event);
    // ...

    private Task Broadcast(string eventType, object data) =>
        hub.Clients.All.SendAsync("IssueEvent", new { eventType, data });
}
```

With `UseConventionalRouting()`, Wolverine automatically creates exchanges per message type and a queue for this handler — no manual wiring needed. The Angular UI receives events via the `@microsoft/signalr` client and updates a reactive signal store.

### TypeScript Generation

[Reinforced.Typings](https://github.com/nicknaso/nicknaso.github.io) generates TypeScript interfaces from the C# contracts at build time. The configuration auto-discovers all record types and value types via assembly reflection:

```csharp
// ReinforcedTypingsConfiguration.cs
builder.SubstituteGenericInterface(typeof(IValueType), (type, resolver) =>
    resolver.ResolveTypeName("string"));
```

Generated files land in `Issues.UI/src/app/generated/` and stay in sync with the C# contracts on every build.

### Strongly-Typed Value Types

The `NForza.Wolverine.ValueTypes.Generators` source generator turns simple declarations:

```csharp
[GuidValue]
public partial record struct IssueId;
```

Into full record structs with JSON serialization, `TryParse`, comparison operators, and Marten-compatible extension methods. A `WolverineValueTypeExtension` is auto-generated to register all JSON converters with Wolverine's serializer.

## Infrastructure

### Docker Compose Services

| Service | Image | Ports | Purpose |
|---|---|---|---|
| PostgreSQL | `postgres:latest` | 5433 | Event store and document storage |
| RabbitMQ | `rabbitmq:4-management` | 5672, 15672 | Message broker between services |

PostgreSQL hosts two databases:
- `issues` - Marten event streams and user documents (IssuesAPI)
- `reporting` - Denormalized read models (IssuesAPI.Reporting)

### Connection Strings

Configured in `appsettings.Development.json` per service:
- IssuesAPI: `Host=localhost;Port=5433;Database=issues;Username=postgres;Password=postgres`
- Reporting: `Host=localhost;Port=5433;Database=reporting;Username=postgres;Password=postgres`

## Testing

Tests use [Alba](https://jasperfx.github.io/alba/) for HTTP integration testing and Wolverine's message tracking to verify the full pipeline:

```csharp
// TrackedHttpCall waits for all cascaded messages to complete
var (tracked, result) = await TrackedHttpCall(x =>
{
    x.Post.Json(command).ToUrl("/issues");
    x.StatusCodeShouldBe(200);
});
```

Each test resets the database to ensure isolation. External transports (RabbitMQ) are disabled during tests.
