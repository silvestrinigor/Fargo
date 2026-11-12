using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemEventCommand : Command
{
    public ItemEventCommand(FargoApiClient client)
        : base("event", "Record and query item events - stock changes for event-tracked articles.")
    {
        Add(new ListItemEventsCommand(client));
        Add(new CreateItemEventCommand(client));
        Add(new DeleteItemEventCommand(client));
        Add(new MoveItemEventCommand(client));
    }
}
