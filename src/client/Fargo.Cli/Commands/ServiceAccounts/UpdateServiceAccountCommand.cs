using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class UpdateServiceAccountCommand : Command
{
    public UpdateServiceAccountCommand(FargoApiClient client)
        : base("update", """
            Update an existing service account. Only the options you pass are changed.

            Examples:
              fargo service-account update 00000000-0000-0000-0000-000000000010 --active false
              fargo service-account update 00000000-0000-0000-0000-000000000010 --add-permissions 4 6 --remove-permissions 2
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The service account GUID."
        };

        var descriptionOption = new Option<string?>("--description")
        {
            Description = "New description."
        };

        var isActiveOption = new Option<bool?>("--active")
        {
            Description = "Set the active state (true or false)."
        };

        var addPermissionsOption = new Option<int[]?>("--add-permissions")
        {
            Description = "ActionType permission codes to grant (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removePermissionsOption = new Option<int[]?>("--remove-permissions")
        {
            Description = "ActionType permission codes to revoke (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var addPartitionsOption = new Option<Guid[]?>("--add-partitions")
        {
            Description = "Partition GUIDs to grant access to (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removePartitionsOption = new Option<Guid[]?>("--remove-partitions")
        {
            Description = "Partition GUIDs to revoke access from (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(descriptionOption);
        Add(isActiveOption);
        Add(addPermissionsOption);
        Add(removePermissionsOption);
        Add(addPartitionsOption);
        Add(removePartitionsOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new ServiceAccountUpdateDto
            {
                Description = parseResult.GetValue(descriptionOption),
                IsActive = parseResult.GetValue(isActiveOption),
                PermissionsToAdd = ToIntList(parseResult.GetValue(addPermissionsOption)),
                PermissionsToRemove = ToIntList(parseResult.GetValue(removePermissionsOption)),
                PartitionAccessesToAdd = ToGuidList(parseResult.GetValue(addPartitionsOption)),
                PartitionAccessesToRemove = ToGuidList(parseResult.GetValue(removePartitionsOption))
            };

            await client.V1.ServiceAccounts[guid].PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Service account updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }

    private static List<int?>? ToIntList(int[]? values)
        => values is { Length: > 0 } ? values.Select(v => (int?)v).ToList() : null;

    private static List<Guid?>? ToGuidList(Guid[]? values)
        => values is { Length: > 0 } ? values.Select(v => (Guid?)v).ToList() : null;
}