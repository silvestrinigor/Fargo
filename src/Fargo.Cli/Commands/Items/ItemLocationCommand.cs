using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemLocationCommand : Command
{
    public ItemLocationCommand(FargoApiClient client)
        : base("location", "Get the location (container chain) for an item")
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

            Console.WriteLine("Location chain (outermost → innermost):");
            foreach (var container in result)
            {
                Console.WriteLine($"  {container.Guid}  article={container.ArticleGuid}");
            }

            return 0;
        });
    }
}
