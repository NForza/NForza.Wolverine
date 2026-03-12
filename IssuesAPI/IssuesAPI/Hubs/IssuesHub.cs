using NForza.Wolverine;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Contracts.Issues.Lifecycle;

namespace Wolverine.Issues.Hubs;

internal class IssuesHub : WolverineHub
{
    public IssuesHub()
    {
        UsePath("/hub/issues");
        Broadcast<IssueCreated>();
        SendToGroup<IssueAssigned>(e => e.IssueId);
        SendToGroup<IssueUnassigned>(e => e.IssueId);
        SendToGroup<IssueClosed>(e => e.IssueId);
        SendToGroup<IssueOpened>(e => e.IssueId);
    }
}
