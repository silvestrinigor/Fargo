using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class SetItemPartitionsCommand : Command
{
    public SetItemPartitionsCommand(FargoApiClient client)
        : base("set-partitions", """
            Change the partitions of an item.

            This is a separate operation from update and requires the ChangeItemPartitions
            permission. You need access to every partition you touch.

            Examples:
              fargo item set-partitions 22222222-2222-2222-2222-222222222222 --add 00000000-0000-0000-0000-000000000002
              fargo item set-partitions 22222222-2222-2222-2222-222222222222 --add 00000000-0000-0000-0000-000000000002 --remove 00000000-0000-0000-0000-000000000005
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The an item GUID."
        };

        var addOption = new Option<Guid[]?>("--add")
        {
            Description = "Partition GUIDs to add (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removeOption = new Option<Guid[]?>("--remove")
        {
            Description = "Partition GUIDs to remove (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(addOption);
        Add(removeOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new PartitionMembershipsChangeDto
            {
                PartitionsToAdd = ToGuidList(parseResult.GetValue(addOption)),
                PartitionsToRemove = ToGuidList(parseResult.GetValue(removeOption))
            };

            await client.V1.Items[guid].Partitions.PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Item partitions updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }

    private static List<Guid?>? ToGuidList(Guid[]? values)
        => values is { Length: > 0 } ? [.. values.Select(v => (Guid?)v)] : null;
}
