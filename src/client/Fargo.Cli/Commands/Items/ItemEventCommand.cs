using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemEventCommand : Command
{
    public ItemEventCommand(FargoApiClient client)
        : base("event", "Query item events - stock changes recorded for event-tracked articles.")
    {
        Add(new ListItemEventsCommand(client));
    }
}
