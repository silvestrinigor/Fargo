using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class GetPartitionCommand : Command
{
    public GetPartitionCommand(FargoApiClient client)
        : base("get", "Get a single partition by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The partition GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var partition = await client.Partitions[guid].GetAsync();

            if (partition is null)
            {
                Console.Error.WriteLine("Partition not found.");
                return 1;
            }

            Console.WriteLine($"GUID:    {partition.Guid}");
            Console.WriteLine($"Name:    {partition.Name}");
            Console.WriteLine($"Parent:  {partition.ParentPartitionGuid}");

            return 0;
        });
    }
}
