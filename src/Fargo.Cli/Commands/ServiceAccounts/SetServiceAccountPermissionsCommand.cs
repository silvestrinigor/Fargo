using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class SetServiceAccountPermissionsCommand : Command
{
    public SetServiceAccountPermissionsCommand(FargoApiClient client)
        : base("set-permissions", """
            Grant and revoke permissions of a service account.

            Permissions are numeric ActionType codes (space-separated). This is a
            separate operation from update and requires the ChangeServiceAccountPermissions
            permission. You must hold every permission you grant or revoke.
            You cannot change your own permissions.

            Examples:
              fargo service-account set-permissions 00000000-0000-0000-0000-000000000010 --add 4 6
              fargo service-account set-permissions 00000000-0000-0000-0000-000000000010 --add 4 --remove 2
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The a service account GUID."
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

            await client.V1.ServiceAccounts[guid].Permissions.PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Service account permissions updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }

    private static List<int?>? ToIntList(int[]? values)
        => values is { Length: > 0 } ? [.. values.Select(v => (int?)v)] : null;
}
