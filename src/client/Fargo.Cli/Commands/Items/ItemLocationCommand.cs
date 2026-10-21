using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemLocationCommand : Command
{
    public ItemLocationCommand(FargoApiClient client)
        : base("location", """
            Show the container chain an item is located in, from the outermost
            container to the innermost.

            Examples:
              fargo item location 22222222-2222-2222-2222-222222222222
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var result = await client.Items[guid].Location.GetAsync();

            if (result is null || result.Count == 0)
            {
                Console.WriteLine("Item has no container location.");
                return 0;
            }

            Console.WriteLine("Location chain (outermost to innermost):");
            Console.WriteLine();

            new TableWriter<Fargo.ClientHttp.Models.ItemDto>()
                .Column("CONTAINER GUID", c => c.Guid?.ToString())
                .Column("ARTICLE", c => c.ArticleGuid?.ToString())
                .Write(result);

            return 0;
        });
    }
}
