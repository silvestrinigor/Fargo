using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemsCommand : Command
{
    public ItemsCommand(FargoApiClient client)
        : base("items", "Manage items")
    {
        Add(new ListItemsCommand(client));
        Add(new GetItemCommand(client));
        Add(new CreateItemCommand(client));
        Add(new UpdateItemCommand(client));
        Add(new DeleteItemCommand(client));
        Add(new ItemLocationCommand(client));
        Add(new ItemMovimentsCommand(client));
        Add(new ItemInventoryCommand(client));
    }
}
