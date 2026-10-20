using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class CreatePartitionCommand : Command
{
    public CreatePartitionCommand(FargoApiClient client)
        : base("create", """
            Create a new partition, optionally nested under a parent.

            Examples:
              fargo partitions create --name "Warehouse A"
              fargo partitions create --name "Aisle 1" --parent 00000000-0000-0000-0000-000000000001
            """)
    {
        var nameOption = new Option<string>("--name")
        {
            Description = "Name of the partition.",
            Required = true
        };

        var parentOption = new Option<Guid?>("--parent")
        {
            Description = "GUID of the parent partition."
        };

        Add(nameOption);
        Add(parentOption);

        SetAction(async parseResult =>
        {
            var name = parseResult.GetValue(nameOption)!;
            var parent = parseResult.GetValue(parentOption);

            var dto = new PartitionCreateDto
            {
                Name = name,
                ParentPartitionGuid = parent
            };

            var guid = await client.Partitions.PostAsync(dto);

            StatusWriter.Success(
                "Partition created.",
                ("GUID", guid?.ToString()),
                ("Name", name));

            return 0;
        });
    }
}
