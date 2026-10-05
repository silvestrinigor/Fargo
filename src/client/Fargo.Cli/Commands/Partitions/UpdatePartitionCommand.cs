using Fargo.Http.Client;
using Fargo.Http.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class UpdatePartitionCommand : Command
{
    public UpdatePartitionCommand(FargoApiClient client)
        : base("update", "Update an existing partition")
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

            Console.WriteLine("Partition updated.");

            return 0;
        });
    }
}
