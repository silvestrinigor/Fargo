using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class DeletePartitionCommand : Command
{
    public DeletePartitionCommand(FargoApiClient client)
        : base("delete", "Delete a partition by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The partition GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.Partitions[guid].DeleteAsync();

            Console.WriteLine("Partition deleted.");

            return 0;
        });
    }
}
