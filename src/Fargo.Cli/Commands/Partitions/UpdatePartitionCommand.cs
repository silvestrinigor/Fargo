using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class UpdatePartitionCommand : Command
{
    public UpdatePartitionCommand(FargoApiClient client)
        : base("update", """
            Update the name or description of an existing partition. Only the options you pass are changed.

            To move a partition under another parent, use 'set-parent'.

            Examples:
              fargo partition update 00000000-0000-0000-0000-000000000002 --name "Warehouse A"
              fargo partition update 00000000-0000-0000-0000-000000000002 --description "Main warehouse"
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The partition GUID."
        };

        var nameOption = new Option<string?>("--name")
        {
            Description = "New name."
        };

        var descriptionOption = new Option<string?>("--description")
        {
            Description = "New description."
        };

        Add(guidArgument);
        Add(nameOption);
        Add(descriptionOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new PartitionUpdateDto
            {
                Name = parseResult.GetValue(nameOption),
                Description = parseResult.GetValue(descriptionOption)
            };

            await client.V1.Partitions[guid].PatchAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Partition updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}