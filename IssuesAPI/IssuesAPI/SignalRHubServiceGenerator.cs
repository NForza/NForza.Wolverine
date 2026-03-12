using System.Text;
using NForza.Wolverine;
using Reinforced.Typings;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Generators;

namespace Wolverine.Issues;

internal static class HubMetadataStore
{
    private static readonly Dictionary<Type, WolverineHub> _hubs = new();

    public static void Register(Type hubType, WolverineHub hub) => _hubs[hubType] = hub;
    public static WolverineHub Get(Type hubType) => _hubs[hubType];
}

public class SignalRHubServiceGenerator : ClassCodeGenerator
{
    public override RtClass GenerateNode(Type element, RtClass result, TypeResolver resolver)
    {
        var hub = HubMetadataStore.Get(element);
        var serviceName = element.Name.Replace("Hub", "") + "HubService";
        var ts = GenerateServiceContent(hub, serviceName);

        // Add raw TS content at the namespace level (not inside a class wrapper)
        Context.Location.CurrentNamespace.CompilationUnits.Add(new RtRaw(ts));

        // Return null to suppress the default class output
        return null!;
    }

    private static string GenerateServiceContent(WolverineHub hub, string serviceName)
    {
        var sb = new StringBuilder();

        // Imports
        sb.AppendLine("import { Injectable, signal } from '@angular/core';");
        sb.AppendLine("import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';");

        foreach (var evt in hub.Events)
        {
            var importPath = ComputeRelativeImport(evt.EventType);
            sb.AppendLine($"import {{ {evt.EventTypeName} }} from '{importPath}';");
        }

        sb.AppendLine();

        // HubEvent interface
        sb.AppendLine("export interface HubEvent<T> {");
        sb.AppendLine("  eventType: string;");
        sb.AppendLine("  data: T;");
        sb.AppendLine("  receivedAt: Date;");
        sb.AppendLine("}");
        sb.AppendLine();

        // Service class
        sb.AppendLine("@Injectable({ providedIn: 'root' })");
        sb.AppendLine($"export class {serviceName} {{");
        sb.AppendLine("  private connection: HubConnection;");
        sb.AppendLine();

        // Per-event typed signals
        foreach (var evt in hub.Events)
        {
            var signalName = ToCamelCase(evt.EventTypeName);
            sb.AppendLine($"  readonly {signalName} = signal<{evt.EventTypeName} | null>(null);");
        }

        // Aggregated events signal
        sb.AppendLine("  readonly allEvents = signal<HubEvent<unknown>[]>([]);");
        sb.AppendLine("  readonly connected = signal(false);");
        sb.AppendLine();

        // Constructor
        sb.AppendLine("  constructor() {");
        sb.AppendLine("    this.connection = new HubConnectionBuilder()");
        sb.AppendLine($"      .withUrl('{hub.Path}')");
        sb.AppendLine("      .withAutomaticReconnect()");
        sb.AppendLine("      .configureLogging(LogLevel.Information)");
        sb.AppendLine("      .build();");
        sb.AppendLine();

        foreach (var evt in hub.Events)
        {
            var signalName = ToCamelCase(evt.EventTypeName);
            sb.AppendLine($"    this.connection.on('{evt.EventTypeName}', (data: {evt.EventTypeName}) => {{");
            sb.AppendLine($"      this.{signalName}.set(data);");
            sb.AppendLine($"      this.allEvents.update(current => [");
            sb.AppendLine($"        {{ eventType: '{evt.EventTypeName}', data, receivedAt: new Date() }},");
            sb.AppendLine($"        ...current,");
            sb.AppendLine($"      ]);");
            sb.AppendLine("    });");
        }

        sb.AppendLine();
        sb.AppendLine("    this.connection.onclose(() => this.connected.set(false));");
        sb.AppendLine("    this.connection.onreconnected(() => this.connected.set(true));");
        sb.AppendLine("    this.start();");
        sb.AppendLine("  }");
        sb.AppendLine();

        // start method
        sb.AppendLine("  private async start(): Promise<void> {");
        sb.AppendLine("    try {");
        sb.AppendLine("      await this.connection.start();");
        sb.AppendLine("      this.connected.set(true);");
        sb.AppendLine("    } catch (err) {");
        sb.AppendLine("      console.error('SignalR connection error:', err);");
        sb.AppendLine("      setTimeout(() => this.start(), 5000);");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string ComputeRelativeImport(Type eventType)
    {
        // The hub service file is at the root of the generated directory (DontIncludeToNamespace),
        // while event types are at namespace-based paths (e.g., Wolverine/Issues/Contracts/Issues/IssueCreated)
        var eventPath = eventType.Namespace?.Replace('.', '/') ?? "";
        return $"./{eventPath}/{eventType.Name}";
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
