using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class UpdateItemCommand : Command
{
    public UpdateItemCommand(FargoApiClient client)
        : base("update", "Update an existing item")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        var containerOption = new Option<Guid?>("--container")
        {
            Description = "New parent container item GUID."
        };

        var removeContainerOption = new Option<bool>("--remove-container")
        {
            Description = "Remove item from its current container."
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
        Add(containerOption);
        Add(removeContainerOption);
        Add(addPartitionsOption);
        Add(removePartitionsOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var container = parseResult.GetValue(containerOption);
            var removeContainer = parseResult.GetValue(removeContainerOption);
            var addPartitions = parseResult.GetValue(addPartitionsOption);
            var removePartitions = parseResult.GetValue(removePartitionsOption);

            var dto = new ItemUpdateDto
            {
                ParentItemContainerGuid = container,
                RemoveFromParentItemContainer = removeContainer ? true : null,
                PartitionsToAdd = addPartitions?.Select(g => (Guid?)g).ToList(),
                PartitionsToRemove = removePartitions?.Select(g => (Guid?)g).ToList()
            };

            await client.Items[guid].PatchAsync(dto);

            Console.WriteLine("Item updated.");

            return 0;
        });
    }
}
