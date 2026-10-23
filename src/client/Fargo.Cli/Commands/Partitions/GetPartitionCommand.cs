using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class GetPartitionCommand : Command
{
    public GetPartitionCommand(FargoApiClient client)
        : base("get", """
            Show the full details of a single partition.

            Examples:
              fargo partition get 00000000-0000-0000-0000-000000000001
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The partition GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var partition = await client.Partitions[guid].GetAsync(cancellationToken: cancellationToken);

            if (partition is null)
            {
                Console.Error.WriteLine("Partition not found.");
                return 1;
            }

            new DetailWriter()
                .Title("Partition")
                .Field("GUID", partition.Guid)
                .Field("Name", partition.Name)
                .Field("Description", partition.Description)
                .Field("Parent", partition.ParentPartitionGuid)
                .Field("Global", partition.IsGlobalPartition)
                .Write();

            return 0;
        });
    }
}
