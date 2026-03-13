# NForza.Wolverine.Generators

A C# [Roslyn source generator](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview) for [Wolverine](https://wolverine.netlify.app/) and [Marten](https://martendb.io/) projects that eliminates boilerplate by generating strongly-typed value types and [SignalR](https://learn.microsoft.com/aspnet/core/signalr/) infrastructure from simple declarations.

## Value Types

Turn a one-line declaration into a full record struct with JSON serialization, `TryParse`, `IParsable<T>` (on .NET 7+), comparison operators, and Marten extension methods:

```csharp
[GuidValue]
public partial record struct IssueId;
```

Supported backing types: `Guid` (`[GuidValue]`), `string` (`[StringValue]`), `int` (`[IntValue]`), `double` (`[DoubleValue]`), `long` (`[LongValue]`), `decimal` (`[DecimalValue]`), `DateOnly` (`[DateOnlyValue]`), `DateTime` (`[DateTimeValue]`), `DateTimeOffset` (`[DateTimeOffsetValue]`).

A `WolverineValueTypeExtension` is auto-generated to register all JSON converters with Wolverine's serializer.

When `Microsoft.AspNetCore.OpenApi` is referenced, a `WolverineValueTypeOpenApiTransformer` is also generated to map value types to their backing type schemas in OpenAPI docs.

The generator includes an analyzer diagnostic **NFW001** that warns on `default(ValueType)` usage, since default-constructed value types are invalid.

## SignalR Hub Generation

Declare events in a `WolverineHub` DSL and get a complete SignalR pipeline generated at compile time:

```csharp
internal class IssuesHub : WolverineHub
{
    public IssuesHub()
    {
        UsePath("/hub/issues");
        Broadcast<IssueCreated>();
        SendToGroup<IssueAssigned>(e => e.IssueId);
        SendToGroup<IssueClosed>(e => e.IssueId);
    }
}
```

The source generator produces three artifacts:
- **Hub class** with typed `SubscribeTo{Entity}` / `UnsubscribeFrom{Entity}` methods (group names are prefixed, e.g. `IssueId:{id}`, to prevent collisions across keys)
- **Bridge class** — Wolverine handlers that forward events to `Clients.All` or `Clients.Group`
- **Registration extension** — `MapHub<T>` call wired into `IEndpointRouteBuilder`

## Angular Client Generation

When combined with [Reinforced.Typings](https://github.com/nicknaso/nicknaso.github.io), the same `WolverineHub` subclass also generates a fully typed Angular SignalR service at build time:

- **Per-event Angular signals** (`issueCreated`, `issueAssigned`, etc.) and an aggregated `allEvents` signal
- **Group subscriptions** — `subscribeToIssue(issueId)` / `unsubscribeFromIssue(issueId)` derived from the `GroupKeyProperty`
- **Auto-reconnect** with automatic resubscription to all tracked groups

---

## Example Application

The repo includes a full event-sourced Issues API as a reference implementation, using [RabbitMQ](https://www.rabbitmq.com/) for messaging and an [Angular](https://angular.dev/) frontend connected via SignalR.

### Architecture Overview

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
| `IssuesAPI/IssuesAPI.Contracts` | Shared events, commands, and value types |
| `IssuesAPI/IssuesAPI.TypeScriptGeneration` | Build-time TypeScript generation via Reinforced.Typings |
| `IssuesAPI/IssuesAPI.Tests` | Integration tests using Alba and Wolverine message tracking |
| `IssuesAPI.Reporting/IssuesAPI.Reporting` | Reporting service with RabbitMQ consumers and query endpoints |
| `IssuesAPI.Reporting/IssuesAPI.Reporting.Tests` | Integration tests for handlers and endpoints |
| `Wolverine.ValueTypes/src` | Attributes and interfaces for strongly-typed value types |
| `Wolverine.Generators` | C# source generator for value types, SignalR hubs, bridges, and registration |
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

Runs all 40 tests across 3 test projects (22 generator + 10 IssuesAPI + 8 reporting).

## Domain Model

### Events

Events are the source of truth, stored in Marten event streams:

| Event | Fields | Description |
|---|---|---|
| `IssueCreated` | `Id`, `OriginatorId`, `OriginatorName`, `Title`, `Description`, `OpenedAt` | A new issue was opened |
| `IssueAssigned` | `IssueId`, `AssigneeId`, `AssigneeName`, `Title` | An issue was assigned to a user |
| `IssueUnassigned` | `IssueId`, `AssigneeId` | An issue was unassigned from a user (emitted before reassignment) |
| `IssueClosed` | `IssueId`, `AssigneeId`, `Closed` | An issue was closed |
| `IssueOpened` | `IssueId`, `AssigneeId`, `Reopened` | A closed issue was reopened |

### Aggregates

The `Issue` aggregate replays events to build current state:

```
IssueCreated    -> sets Id, Title, Description, OriginatorId, IsOpen=true
IssueUnassigned -> sets AssigneeId=null
IssueAssigned   -> sets AssigneeId
IssueClosed     -> sets IsOpen=false
IssueOpened     -> sets IsOpen=true
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

Events flow from the Marten outbox through RabbitMQ back into the application, where the generated bridge forwards them to SignalR (see [SignalR Hub Generation](#signalr-hub-generation) above). With `UseConventionalRouting()`, Wolverine automatically creates exchanges per message type and a queue for the bridge — no manual wiring needed.

The Angular frontend uses the generated `IssuesHubService` (see [Angular Client Generation](#angular-client-generation) above). Generated files land in `Issues.UI/src/app/generated/` and stay in sync with the C# contracts on every build. During development, Angular's proxy config routes `/hub/*` to the API at `http://localhost:5035` with WebSocket support.

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
