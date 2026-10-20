using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemInventoryCommand : Command
{
    public ItemInventoryCommand(FargoApiClient client)
        : base("inventory", """
            Show the article inventory contained inside a container item.

            Examples:
              fargo items inventory 22222222-2222-2222-2222-222222222222
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The container item GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var result = await client.Items[guid].Inventory.GetAsync();

            new TableWriter<Fargo.ClientHttp.Models.ItemContainerInventoryDto>()
                .Column("ARTICLE GUID", e => e.ArticleGuid?.ToString())
                .Column("COUNT", e => e.TotalCount?.ToString())
                .EmptyMessage("Container is empty.")
                .Write(result);

            return 0;
        });
    }
}
