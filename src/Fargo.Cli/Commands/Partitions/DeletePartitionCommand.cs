using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class DeletePartitionCommand : Command
{
    public DeletePartitionCommand(FargoApiClient client)
        : base("delete", """
            Delete a partition by GUID.

            Examples:
              fargo partition delete 00000000-0000-0000-0000-000000000002
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

            await client.V1.Partitions[guid].DeleteAsync(cancellationToken: cancellationToken);

            StatusWriter.Success("Partition deleted.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
