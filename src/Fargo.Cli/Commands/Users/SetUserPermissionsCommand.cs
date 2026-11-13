using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class SetUserPermissionsCommand : Command
{
    public SetUserPermissionsCommand(FargoApiClient client)
        : base("set-permissions", """
            Grant and revoke permissions of a user.

            Permissions are numeric ActionType codes (space-separated). This is a
            separate operation from update and requires the ChangeUserPermissions
            permission. You must hold every permission you grant or revoke.
            You cannot change your own permissions.

            Examples:
              fargo user set-permissions 00000000-0000-0000-0000-000000000004 --add 4 6
              fargo user set-permissions 00000000-0000-0000-0000-000000000004 --add 4 --remove 2
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The a user GUID."
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

            await client.V1.Users[guid].Permissions.PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("User permissions updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }

    private static List<int?>? ToIntList(int[]? values)
        => values is { Length: > 0 } ? [.. values.Select(v => (int?)v)] : null;
}
