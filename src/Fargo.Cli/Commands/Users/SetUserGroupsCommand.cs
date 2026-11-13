using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class SetUserGroupsCommand : Command
{
    public SetUserGroupsCommand(FargoApiClient client)
        : base("set-groups", """
            Add a user to, and remove it from, user groups.

            Joining a group hands the user the permissions of that group and of its
            ancestors, so you must hold all of them. This is a separate operation
            from update and requires the ChangeUserGroupMembers permission.

            Examples:
              fargo user set-groups 00000000-0000-0000-0000-000000000004 --add 00000000-0000-0000-0000-000000000003
              fargo user set-groups 00000000-0000-0000-0000-000000000004 --remove 00000000-0000-0000-0000-000000000003
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        var addOption = new Option<Guid[]?>("--add")
        {
            Description = "User group GUIDs to add the user to (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removeOption = new Option<Guid[]?>("--remove")
        {
            Description = "User group GUIDs to remove the user from (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(addOption);
        Add(removeOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new UserGroupsChangeDto
            {
                UserGroupsToAdd = ToGuidList(parseResult.GetValue(addOption)),
                UserGroupsToRemove = ToGuidList(parseResult.GetValue(removeOption))
            };

            await client.V1.Users[guid].UserGroups.PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("User groups updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }

    private static List<Guid?>? ToGuidList(Guid[]? values)
        => values is { Length: > 0 } ? [.. values.Select(v => (Guid?)v)] : null;
}
