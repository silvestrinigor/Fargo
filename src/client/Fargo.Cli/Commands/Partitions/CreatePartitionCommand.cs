using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class CreatePartitionCommand : Command
{
    public CreatePartitionCommand(FargoApiClient client)
        : base("create", "Create a new partition")
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

            Console.WriteLine($"Partition created: {guid}");

            return 0;
        });
    }
}
