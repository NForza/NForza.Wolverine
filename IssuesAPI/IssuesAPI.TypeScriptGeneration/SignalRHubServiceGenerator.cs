using System.Text;
using NForza.Wolverine;
using Reinforced.Typings;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Generators;

namespace Wolverine.Issues.TypeScriptGeneration;

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
        var groupKeys = hub.Events
            .Where(e => !e.IsBroadcast && e.GroupKeyProperty != null)
            .Select(e => e.GroupKeyProperty!)
            .Distinct()
            .ToList();

        var hasGroups = groupKeys.Count > 0;
        var sb = new StringBuilder();

        // Imports
        sb.AppendLine("import { Injectable, signal } from '@angular/core';");
        sb.AppendLine("import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr';");

        foreach (var evt in hub.Events)
        {
            var importPath = ComputeRelativeImport(evt);
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

        foreach (var key in groupKeys)
        {
            var entityName = StripIdSuffix(key);
            sb.AppendLine($"  private subscribed{entityName}Ids = new Set<string>();");
        }

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

        if (hasGroups)
        {
            sb.AppendLine("    this.connection.onreconnected(() => {");
            sb.AppendLine("      this.connected.set(true);");
            sb.AppendLine("      this.resubscribe();");
            sb.AppendLine("    });");
        }
        else
        {
            sb.AppendLine("    this.connection.onreconnected(() => this.connected.set(true));");
        }

        sb.AppendLine("    this.start();");
        sb.AppendLine("  }");
        sb.AppendLine();

        // Per-group-key subscribe/unsubscribe methods
        foreach (var key in groupKeys)
        {
            var entityName = StripIdSuffix(key);
            var paramName = ToCamelCase(key);

            sb.AppendLine($"  async subscribeTo{entityName}({paramName}: string): Promise<void> {{");
            sb.AppendLine($"    this.subscribed{entityName}Ids.add({paramName});");
            sb.AppendLine("    if (this.connected()) {");
            sb.AppendLine($"      await this.connection.invoke('SubscribeTo{entityName}', {paramName});");
            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine();
            sb.AppendLine($"  async unsubscribeFrom{entityName}({paramName}: string): Promise<void> {{");
            sb.AppendLine($"    this.subscribed{entityName}Ids.delete({paramName});");
            sb.AppendLine("    if (this.connected()) {");
            sb.AppendLine($"      await this.connection.invoke('UnsubscribeFrom{entityName}', {paramName});");
            sb.AppendLine("    }");
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        // Resubscribe on reconnect
        if (hasGroups)
        {
            sb.AppendLine("  private async resubscribe(): Promise<void> {");
            foreach (var key in groupKeys)
            {
                var entityName = StripIdSuffix(key);
                var idVar = ToCamelCase(key);
                sb.AppendLine($"    for (const {idVar} of this.subscribed{entityName}Ids) {{");
                sb.AppendLine($"      await this.connection.invoke('SubscribeTo{entityName}', {idVar});");
                sb.AppendLine("    }");
            }
            sb.AppendLine("  }");
            sb.AppendLine();
        }

        // start method
        sb.AppendLine("  private async start(): Promise<void> {");
        sb.AppendLine("    try {");
        sb.AppendLine("      await this.connection.start();");
        sb.AppendLine("      this.connected.set(true);");

        if (hasGroups)
        {
            sb.AppendLine("      await this.resubscribe();");
        }

        sb.AppendLine("    } catch (err) {");
        sb.AppendLine("      console.error('SignalR connection error:', err);");
        sb.AppendLine("      setTimeout(() => this.start(), 5000);");
        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string ComputeRelativeImport(WolverineHubEventConfig evt)
    {
        // The hub service file is at the root of the generated directory (DontIncludeToNamespace),
        // while event types are at namespace-based paths (e.g., Wolverine/Issues/Contracts/Issues/IssueCreated)
        var eventPath = evt.EventTypeNamespace.Replace('.', '/');
        return $"./{eventPath}/{evt.EventTypeName}";
    }

    private static string StripIdSuffix(string propertyName)
    {
        if (propertyName.EndsWith("Id"))
            return propertyName[..^2];
        return propertyName;
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
