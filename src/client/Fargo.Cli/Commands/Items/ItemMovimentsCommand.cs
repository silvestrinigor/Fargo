using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemMovimentsCommand : Command
{
    public ItemMovimentsCommand(FargoApiClient client)
        : base("moviments", "Get the movement history for an item")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var result = await client.Items[guid].Moviments.GetAsync();

            if (result is null || result.Count == 0)
            {
                Console.WriteLine("No movement history found.");
                return 0;
            }

            foreach (var moviment in result)
            {
                Console.WriteLine($"{moviment.OccurredAt:O}  container={moviment.MovedToItemContainerGuid}");
            }

            return 0;
        });
    }
}
