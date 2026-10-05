using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class PartitionsCommand : Command
{
    public PartitionsCommand(FargoApiClient client)
        : base("partitions", "Manage partitions")
    {
        Add(new ListPartitionsCommand(client));
        Add(new GetPartitionCommand(client));
        Add(new CreatePartitionCommand(client));
        Add(new UpdatePartitionCommand(client));
        Add(new DeletePartitionCommand(client));
    }
}
