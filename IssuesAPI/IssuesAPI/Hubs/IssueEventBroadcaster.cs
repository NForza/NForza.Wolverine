using Microsoft.AspNetCore.SignalR;

namespace Wolverine.Issues.Hubs;

public class IssueEventBroadcaster(IHubContext<IssuesHub> hub)
{
    public Task Broadcast(string eventType, object data) =>
        hub.Clients.All.SendAsync("IssueEvent", new { eventType, data });
}
