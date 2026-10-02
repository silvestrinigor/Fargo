using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class UsersCommand : Command
{
    public UsersCommand(FargoApiClient client)
        : base("users", "Manage users")
    {
        Add(new ListUsersCommand(client));
        Add(new GetUserCommand(client));
        Add(new CreateUserCommand(client));
        Add(new UpdateUserCommand(client));
        Add(new DeleteUserCommand(client));
    }
}
