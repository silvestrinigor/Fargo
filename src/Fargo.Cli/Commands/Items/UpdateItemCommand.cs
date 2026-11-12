using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class UpdateItemCommand : Command
{
    public UpdateItemCommand(FargoApiClient client)
        : base("update", """
            Update an existing item. Only the options you pass are changed.

            Examples:
              fargo item update 22222222-2222-2222-2222-222222222222 --add-partitions 00000000-0000-0000-0000-000000000002
              fargo item update 22222222-2222-2222-2222-222222222222 --remove-partitions 00000000-0000-0000-0000-000000000002
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        var addPartitionsOption = new Option<Guid[]?>("--add-partitions")
        {
            Description = "Partition GUIDs to add (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var removePartitionsOption = new Option<Guid[]?>("--remove-partitions")
        {
            Description = "Partition GUIDs to remove (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(guidArgument);
        Add(addPartitionsOption);
        Add(removePartitionsOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var addPartitions = parseResult.GetValue(addPartitionsOption);
            var removePartitions = parseResult.GetValue(removePartitionsOption);

            var dto = new ItemUpdateDto
            {
                PartitionsToAdd = addPartitions?.Select(g => (Guid?)g).ToList(),
                PartitionsToRemove = removePartitions?.Select(g => (Guid?)g).ToList()
            };

            await client.V1.Items[guid].PatchAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Item updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
