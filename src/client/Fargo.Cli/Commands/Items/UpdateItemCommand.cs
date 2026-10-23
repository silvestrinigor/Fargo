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
              fargo item update 22222222-2222-2222-2222-222222222222 --container 33333333-3333-3333-3333-333333333333
              fargo item update 22222222-2222-2222-2222-222222222222 --remove-container
            """)
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
            Description = "Remove the item from its current container."
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

        SetAction(async (parseResult, cancellationToken) =>
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

            await client.Items[guid].PatchAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Item updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
