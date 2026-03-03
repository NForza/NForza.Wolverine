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
        Broadcast<IssueAssigned>();
        Broadcast<IssueUnassigned>();
        Broadcast<IssueClosed>();
        Broadcast<IssueOpened>();
    }
}
