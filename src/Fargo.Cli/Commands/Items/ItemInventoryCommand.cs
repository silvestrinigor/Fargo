using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemInventoryCommand : Command
{
    public ItemInventoryCommand(FargoApiClient client)
        : base("inventory", "Get the article inventory inside a container item")
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

            if (result is null || result.Count == 0)
            {
                Console.WriteLine("Container is empty.");
                return 0;
            }

            Console.WriteLine($"{"Article GUID",-40}  {"Count"}");
            Console.WriteLine(new string('-', 50));

            foreach (var entry in result)
            {
                Console.WriteLine($"{entry.ArticleGuid,-40}  {entry.TotalCount}");
            }

            return 0;
        });
    }
}
