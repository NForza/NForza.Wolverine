using System.Collections.Immutable;

namespace NForza.Wolverine.Generators;

internal class SignalREventInfo
{
    public string EventTypeName { get; set; } = "";
    public string EventTypeNamespace { get; set; } = "";
    public string FullyQualifiedName => string.IsNullOrEmpty(EventTypeNamespace) ? EventTypeName : EventTypeNamespace + "." + EventTypeName;
    public bool IsBroadcast { get; set; }
    public string? GroupKeyProperty { get; set; }
}

internal class SignalRHubInfo
{
    public string HubClassName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string FullyQualifiedName => string.IsNullOrEmpty(Namespace) ? HubClassName : Namespace + "." + HubClassName;
    public string GeneratedHubName => HubClassName + "SignalR";
    public string GeneratedHubFullyQualifiedName => string.IsNullOrEmpty(Namespace) ? GeneratedHubName : Namespace + "." + GeneratedHubName;
    public string Path { get; set; } = "";
    public ImmutableArray<SignalREventInfo> Events { get; set; } = ImmutableArray<SignalREventInfo>.Empty;
}
