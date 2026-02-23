using WolverineGettingStarted.Users;

namespace Wolverine.Issues.Users;

public class User
{
    public UserId Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
