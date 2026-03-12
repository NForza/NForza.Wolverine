using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NForza.Wolverine.Generators.CodeGeneration;

internal static class SignalRBridgeTemplates
{
    public static string GenerateHub(SignalRHubInfo hubInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Microsoft.AspNetCore.SignalR;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(hubInfo.Namespace))
        {
            sb.AppendLine($"namespace {hubInfo.Namespace};");
            sb.AppendLine();
        }

        var groupKeys = hubInfo.Events
            .Where(e => !e.IsBroadcast && e.GroupKeyProperty != null)
            .Select(e => e.GroupKeyProperty!)
            .Distinct()
            .ToList();

        if (groupKeys.Count == 0)
        {
            sb.AppendLine($"public class {hubInfo.GeneratedHubName} : Hub;");
        }
        else
        {
            sb.AppendLine($"public class {hubInfo.GeneratedHubName} : Hub");
            sb.AppendLine("{");
            foreach (var key in groupKeys)
            {
                var entityName = StripIdSuffix(key);
                var paramName = char.ToLowerInvariant(key[0]) + key.Substring(1);
                sb.AppendLine($"    public Task SubscribeTo{entityName}(string {paramName}) => Groups.AddToGroupAsync(Context.ConnectionId, $\"{key}:{{{paramName}}}\");");
                sb.AppendLine($"    public Task UnsubscribeFrom{entityName}(string {paramName}) => Groups.RemoveFromGroupAsync(Context.ConnectionId, $\"{key}:{{{paramName}}}\");");
            }
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    public static string GenerateBridge(SignalRHubInfo hubInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.AspNetCore.SignalR;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(hubInfo.Namespace))
        {
            sb.AppendLine($"namespace {hubInfo.Namespace};");
            sb.AppendLine();
        }

        sb.AppendLine($"public class {hubInfo.HubClassName}SignalRBridge(IHubContext<global::{hubInfo.GeneratedHubFullyQualifiedName}> hub)");
        sb.AppendLine("{");

        foreach (var evt in hubInfo.Events)
        {
            if (evt.IsBroadcast)
            {
                sb.AppendLine($"    public Task Handle(global::{evt.FullyQualifiedName} @event) => hub.Clients.All.SendAsync(\"{evt.EventTypeName}\", @event);");
            }
            else
            {
                sb.AppendLine($"    public Task Handle(global::{evt.FullyQualifiedName} @event) => hub.Clients.Group($\"{evt.GroupKeyProperty}:{{@event.{evt.GroupKeyProperty}}}\").SendAsync(\"{evt.EventTypeName}\", @event);");
            }
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    public static string GenerateRegistration(ImmutableArray<SignalRHubInfo> hubs)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Microsoft.AspNetCore.Builder;");
        sb.AppendLine("using Microsoft.AspNetCore.SignalR;");
        sb.AppendLine();
        sb.AppendLine("namespace NForza.Wolverine;");
        sb.AppendLine();
        sb.AppendLine("public static class WolverineHubExtensions");
        sb.AppendLine("{");
        sb.AppendLine("    public static void MapWolverineHubs(this WebApplication app)");
        sb.AppendLine("    {");

        foreach (var hub in hubs)
        {
            sb.AppendLine($"        app.MapHub<global::{hub.GeneratedHubFullyQualifiedName}>(\"{hub.Path}\");");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string StripIdSuffix(string propertyName)
    {
        if (propertyName.EndsWith("Id"))
            return propertyName.Substring(0, propertyName.Length - 2);
        return propertyName;
    }
}
