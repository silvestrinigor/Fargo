using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class SetUserGroupPermissionsCommand : Command
{
    public SetUserGroupPermissionsCommand(FargoApiClient client)
        : base("set-permissions", """
            Grant and revoke permissions of a user group.

            Permissions are numeric ActionType codes (space-separated). This is a
            separate operation from update and requires the ChangeUserGroupPermissions
            permission. You must hold every permission you grant or revoke.

            Examples:
              fargo user-group set-permissions 00000000-0000-0000-0000-000000000003 --add 4 6
              fargo user-group set-permissions 00000000-0000-0000-0000-000000000003 --add 4 --remove 2
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The a user group GUID."
        };

        var addOption = new Option<int[]?>("--add")
        {
            Description = "ActionType permission codes to grant (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removeOption = new Option<int[]?>("--remove")
        {
            Description = "ActionType permission codes to revoke (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(addOption);
        Add(removeOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new PermissionsChangeDto
            {
                PermissionsToAdd = ToIntList(parseResult.GetValue(addOption)),
                PermissionsToRemove = ToIntList(parseResult.GetValue(removeOption))
            };

            await client.V1.UserGroups[guid].Permissions.PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("User group permissions updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }

    private static List<int?>? ToIntList(int[]? values)
        => values is { Length: > 0 } ? [.. values.Select(v => (int?)v)] : null;
}
