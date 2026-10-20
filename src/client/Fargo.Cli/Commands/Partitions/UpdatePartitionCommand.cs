using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class UpdatePartitionCommand : Command
{
    public UpdatePartitionCommand(FargoApiClient client)
        : base("update", """
            Update an existing partition. Only the options you pass are changed.

            Examples:
              fargo partitions update 00000000-0000-0000-0000-000000000002 --parent 00000000-0000-0000-0000-000000000001
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The partition GUID."
        };

        var parentOption = new Option<Guid?>("--parent")
        {
            Description = "New parent partition GUID."
        };

        Add(guidArgument);
        Add(parentOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var parent = parseResult.GetValue(parentOption);

            var dto = new PartitionUpdateDto
            {
                ParentPartitionGuid = parent
            };

            await client.Partitions[guid].PatchAsync(dto);

            StatusWriter.Success("Partition updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
