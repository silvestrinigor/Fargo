using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemMovimentsCommand : Command
{
    public ItemMovimentsCommand(FargoApiClient client)
        : base("moviments", """
            Show the movement history for an item (when it changed containers).

            Examples:
              fargo items moviments 22222222-2222-2222-2222-222222222222
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

            var result = await client.Items[guid].Moviments.GetAsync();

            new TableWriter<Fargo.ClientHttp.Models.ItemMovimentDto>()
                .Column("OCCURRED AT", m => m.OccurredAt?.ToString("u"))
                .Column("MOVED TO CONTAINER", m => m.MovedToItemContainerGuid?.ToString())
                .EmptyMessage("No movement history found.")
                .Write(result);

            return 0;
        });
    }
}
