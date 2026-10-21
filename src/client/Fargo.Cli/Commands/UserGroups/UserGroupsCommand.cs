using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class UserGroupsCommand : Command
{
    public UserGroupsCommand(FargoApiClient client)
        : base("user-group", "Manage user groups - collections of users that share permissions.")
    {
        Add(new ListUserGroupsCommand(client));
        Add(new GetUserGroupCommand(client));
        Add(new CreateUserGroupCommand(client));
        Add(new UpdateUserGroupCommand(client));
        Add(new DeleteUserGroupCommand(client));
    }
}
