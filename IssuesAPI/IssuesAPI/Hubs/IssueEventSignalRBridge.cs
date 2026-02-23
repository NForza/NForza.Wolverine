using Microsoft.AspNetCore.SignalR;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Contracts.Issues.Lifecycle;

namespace Wolverine.Issues.Hubs;

public class IssueEventSignalRBridge(IHubContext<IssuesHub> hub)
{
    public Task Handle(IssueCreated @event) => hub.Clients.All.SendAsync(nameof(IssueCreated), @event);
    public Task Handle(IssueAssigned @event) => hub.Clients.All.SendAsync(nameof(IssueAssigned), @event);
    public Task Handle(IssueUnassigned @event) => hub.Clients.All.SendAsync(nameof(IssueUnassigned), @event);
    public Task Handle(IssueClosed @event) => hub.Clients.All.SendAsync(nameof(IssueClosed), @event);
    public Task Handle(IssueOpened @event) => hub.Clients.All.SendAsync(nameof(IssueOpened), @event);
}
