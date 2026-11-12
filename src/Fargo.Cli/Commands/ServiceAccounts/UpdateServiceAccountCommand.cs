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

            Partition membership (the partitions the account belongs to) and
            partition access (the partitions the account may reach) are managed
            independently.

            Examples:
              fargo service-account update 00000000-0000-0000-0000-000000000010 --description "CI runner"
              fargo service-account update 00000000-0000-0000-0000-000000000010 --add-permissions 4 6 --remove-permissions 2
              fargo service-account update 00000000-0000-0000-0000-000000000010 --add-partition-memberships 00000000-0000-0000-0000-000000000002
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

        var addPartitionMembershipsOption = new Option<Guid[]?>("--add-partition-memberships")
        {
            Description = "Partition GUIDs to add to the account's membership (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removePartitionMembershipsOption = new Option<Guid[]?>("--remove-partition-memberships")
        {
            Description = "Partition GUIDs to remove from the account's membership (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var addPartitionAccessesOption = new Option<Guid[]?>("--add-partition-accesses", "--add-partitions")
        {
            Description = "Partition GUIDs to grant access to (space-separated). Alias: --add-partitions.",
            AllowMultipleArgumentsPerToken = true
        };

        var removePartitionAccessesOption = new Option<Guid[]?>("--remove-partition-accesses", "--remove-partitions")
        {
            Description = "Partition GUIDs to revoke access from (space-separated). Alias: --remove-partitions.",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(descriptionOption);
        Add(addPermissionsOption);
        Add(removePermissionsOption);
        Add(addPartitionMembershipsOption);
        Add(removePartitionMembershipsOption);
        Add(addPartitionAccessesOption);
        Add(removePartitionAccessesOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new ServiceAccountUpdateDto
            {
                Description = parseResult.GetValue(descriptionOption),
                PermissionsToAdd = ToIntList(parseResult.GetValue(addPermissionsOption)),
                PermissionsToRemove = ToIntList(parseResult.GetValue(removePermissionsOption)),
                PartitionsToAdd = ToGuidList(parseResult.GetValue(addPartitionMembershipsOption)),
                PartitionsToRemove = ToGuidList(parseResult.GetValue(removePartitionMembershipsOption)),
                PartitionAccessesToAdd = ToGuidList(parseResult.GetValue(addPartitionAccessesOption)),
                PartitionAccessesToRemove = ToGuidList(parseResult.GetValue(removePartitionAccessesOption))
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
