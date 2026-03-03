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

        sb.AppendLine($"public class {hubInfo.GeneratedHubName} : Hub;");

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
                sb.AppendLine($"    public Task Handle(global::{evt.FullyQualifiedName} @event) => hub.Clients.Group(@event.{evt.GroupKeyProperty}.ToString()).SendAsync(\"{evt.EventTypeName}\", @event);");
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
}
