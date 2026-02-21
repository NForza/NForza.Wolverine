using System.Net.Mail;
using Marten;

namespace WolverineGettingStarted.Issues;

public static class IssueCreatedHandler
{
    public static async Task Handle(IssueCreated created, IQuerySession session)
    {
        var issue = await session.LoadAsync<Issue>(created.Id);
        if (issue is null) return;

        var message = await BuildEmailMessage(issue);
        using var client = new SmtpClient();
        client.Send(message);
    }

    internal static Task<MailMessage> BuildEmailMessage(Issue issue)
    {
        // Build up a templated email message, with
        // some sort of async method to look up additional
        // data just so we can show off an async
        // Wolverine Handler
        return Task.FromResult(new MailMessage());
    }
}
