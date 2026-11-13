using Fargo.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class UsersCommand : Command
{
    public UsersCommand(FargoApiClient client)
        : base("user", "Manage user accounts (list, inspect, create, update, activate or deactivate, delete).")
    {
        Add(new ListUsersCommand(client));
        Add(new GetUserCommand(client));
        Add(new CreateUserCommand(client));
        Add(new UpdateUserCommand(client));
        Add(new SetUserActiveCommand(client));
        Add(new SetUserPermissionsCommand(client));
        Add(new SetUserGroupsCommand(client));
        Add(new SetUserPartitionsCommand(client));
        Add(new ResetUserPasswordCommand(client));
        Add(new SetUserPasswordExpirationCommand(client));
        Add(new RemoveUserPasswordExpirationCommand(client));
        Add(new DeleteUserCommand(client));
    }
}
