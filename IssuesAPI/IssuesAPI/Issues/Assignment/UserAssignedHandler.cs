using System.Net.Mail;
using Marten;
using Wolverine.Issues.Contracts.Issues;
using Wolverine.Issues.Issues.Model;
using Wolverine.Issues.Users;

namespace Wolverine.Issues.Issues.Assignment;

public static class UserAssignedHandler
{
    public static async Task Handle(IssueAssigned assigned, IQuerySession session)
    {
        var issue = await session.LoadAsync<Issue>(assigned.IssueId);
        if (issue is null) return;

        var user = await session.LoadAsync<User>(assigned.AssigneeId);
        if (user is null) return;

        var message = BuildEmailMessage(issue, user);
        using var client = new SmtpClient();
        client.SendAsync(message, "some token");
    }

    public static MailMessage BuildEmailMessage(Issue issue, User user)
    {
        // Build up a templated email message
        return new MailMessage();
    }
}
