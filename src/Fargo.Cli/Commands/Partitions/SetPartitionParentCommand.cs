using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class SetPartitionParentCommand : Command
{
    public SetPartitionParentCommand(FargoApiClient client)
        : base("set-parent", """
            Move a partition under another parent.

            This changes who can access the partition. A partition always has a parent,
            so it can be moved but not detached. This is a separate operation from update
            and requires the ChangePartitionParent permission.

            Examples:
              fargo partition set-parent 00000000-0000-0000-0000-000000000002 --parent 00000000-0000-0000-0000-000000000001
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The partition GUID."
        };

        var parentOption = new Option<Guid>("--parent")
        {
            Description = "GUID of the new parent partition.",
            Required = true
        };

        Add(guidArgument);
        Add(parentOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var parent = parseResult.GetValue(parentOption);

            await client.V1.Partitions[guid].Parent.PutAsync(
                new ParentPartitionDto { ParentPartitionGuid = parent },
                cancellationToken: cancellationToken);

            StatusWriter.Success("Partition parent set.", ("GUID", guid.ToString()), ("Parent", parent.ToString()));

            return 0;
        });
    }
}
