using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class SetUserGroupPartitionsCommand : Command
{
    public SetUserGroupPartitionsCommand(FargoApiClient client)
        : base("set-partitions", """
            Change the partitions of a user group.

            Membership is the partitions it belongs to; access is the partitions it
            may reach. This is a separate operation from update and requires the
            ChangeUserGroupPartitions permission. You need access to every partition you touch.

            Examples:
              fargo user-group set-partitions 00000000-0000-0000-0000-000000000003 --add-accesses 00000000-0000-0000-0000-000000000002
              fargo user-group set-partitions 00000000-0000-0000-0000-000000000003 --add-memberships 00000000-0000-0000-0000-000000000002 --remove-accesses 00000000-0000-0000-0000-000000000005
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The a user group GUID."
        };

        var addMembershipsOption = new Option<Guid[]?>("--add-memberships")
        {
            Description = "Partition GUIDs to add to the membership (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removeMembershipsOption = new Option<Guid[]?>("--remove-memberships")
        {
            Description = "Partition GUIDs to remove from the membership (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var addAccessesOption = new Option<Guid[]?>("--add-accesses")
        {
            Description = "Partition GUIDs to grant access to (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removeAccessesOption = new Option<Guid[]?>("--remove-accesses")
        {
            Description = "Partition GUIDs to revoke access from (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(addMembershipsOption);
        Add(removeMembershipsOption);
        Add(addAccessesOption);
        Add(removeAccessesOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new PartitionsChangeDto
            {
                PartitionsToAdd = ToGuidList(parseResult.GetValue(addMembershipsOption)),
                PartitionsToRemove = ToGuidList(parseResult.GetValue(removeMembershipsOption)),
                PartitionAccessesToAdd = ToGuidList(parseResult.GetValue(addAccessesOption)),
                PartitionAccessesToRemove = ToGuidList(parseResult.GetValue(removeAccessesOption))
            };

            await client.V1.UserGroups[guid].Partitions.PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("User group partitions updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }

    private static List<Guid?>? ToGuidList(Guid[]? values)
        => values is { Length: > 0 } ? values.Select(v => (Guid?)v).ToList() : null;
}