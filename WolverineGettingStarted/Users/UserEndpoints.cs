using Marten;
using Wolverine.Http;

namespace WolverineGettingStarted.Users;

public record CreateUser(string Name, string Email);

public static class UserEndpoints
{
    [WolverinePost("/api/users")]
    public static async Task<User> CreateUser(CreateUser command, IDocumentSession session)
    {
        var user = new User
        {
            Id = new UserId(),
            Name = command.Name,
            Email = command.Email
        };

        session.Store(user);
        await session.SaveChangesAsync();

        return user;
    }

    [WolverineGet("/api/users/{id}")]
    public static Task<User?> GetUser(UserId id, IQuerySession session)
    {
        return session.Query<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
    }
}
