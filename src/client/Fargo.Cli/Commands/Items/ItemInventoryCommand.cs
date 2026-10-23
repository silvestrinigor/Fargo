using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ItemInventoryCommand : Command
{
    public ItemInventoryCommand(FargoApiClient client)
        : base("inventory", """
            Show the article inventory contained inside a container item.

            Pass --at to reconstruct the inventory as it was at a specific
            instant, using the item containment history, instead of the current
            inventory.

            Examples:
              fargo item inventory 22222222-2222-2222-2222-222222222222
              fargo item inventory 22222222-2222-2222-2222-222222222222 --at 2026-03-01T12:00:00Z
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The container item GUID."
        };

        var atOption = new Option<DateTimeOffset?>("--at")
        {
            Description = "Instant (ISO 8601, e.g. 2026-03-01T12:00:00Z) at which to evaluate the inventory. Defaults to now."
        };

        Add(guidArgument);
        Add(atOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var at = parseResult.GetValue(atOption);

            var result = await client.Items[guid].Inventory.GetAsync(request =>
            {
                if (at.HasValue)
                {
                    request.QueryParameters.At = at.Value;
                }
            }, cancellationToken);

            if (at.HasValue)
            {
                Console.WriteLine($"Inventory as of {at.Value:u}:");
                Console.WriteLine();
            }

            new TableWriter<Fargo.ClientHttp.Models.ItemContainerInventoryDto>()
                .Column("ARTICLE GUID", e => e.ArticleGuid?.ToString())
                .Column("COUNT", e => e.TotalCount?.ToString())
                .EmptyMessage(at.HasValue
                    ? "Container was empty at the requested time."
                    : "Container is empty.")
                .Write(result);

            return 0;
        });
    }
}
