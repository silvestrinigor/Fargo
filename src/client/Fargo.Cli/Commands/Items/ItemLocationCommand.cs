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

            Pass --as-of to reconstruct the location as it was at a specific
            instant, using the item's containment history, instead of the
            current location.

            Examples:
              fargo item location 22222222-2222-2222-2222-222222222222
              fargo item location 22222222-2222-2222-2222-222222222222 --as-of 2026-03-01T12:00:00Z
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The item GUID."
        };

        var asOfOption = new Option<DateTimeOffset?>("--as-of")
        {
            Description = "Instant (ISO 8601, e.g. 2026-03-01T12:00:00Z) at which to evaluate the location. Defaults to now."
        };

        Add(guidArgument);
        Add(asOfOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var asOf = parseResult.GetValue(asOfOption);

            var result = await client.Items[guid].Location.GetAsync(request =>
            {
                if (asOf.HasValue)
                {
                    request.QueryParameters.AsOf = asOf.Value;
                }
            });

            if (result is null || result.Count == 0)
            {
                Console.WriteLine(asOf.HasValue
                    ? "Item had no container location at the requested time."
                    : "Item has no container location.");
                return 0;
            }

            Console.WriteLine(asOf.HasValue
                ? $"Location chain as of {asOf.Value:u} (outermost to innermost):"
                : "Location chain (outermost to innermost):");
            Console.WriteLine();

            new TableWriter<Fargo.ClientHttp.Models.ItemDto>()
                .Column("CONTAINER GUID", c => c.Guid?.ToString())
                .Column("ARTICLE", c => c.ArticleGuid?.ToString())
                .Write(result);

            return 0;
        });
    }
}
