using Fargo.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemsCommand : Command
{
    public ItemsCommand(FargoApiClient client)
        : base("item", "Manage items - physical instances of articles and their locations.")
    {
        Add(new ListItemsCommand(client));
        Add(new GetItemCommand(client));
        Add(new CreateItemCommand(client));
        Add(new SetItemPartitionsCommand(client));
        Add(new MoveItemCommand(client));
        Add(new RemoveItemFromContainerCommand(client));
        Add(new DeleteItemCommand(client));
        Add(new ItemLocationCommand(client));
        Add(new ItemMovimentsCommand(client));
        Add(new ItemInventoryCommand(client));
        Add(new ItemEventCommand(client));
    }
}
