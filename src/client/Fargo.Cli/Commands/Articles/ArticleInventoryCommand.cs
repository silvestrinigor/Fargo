using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticleInventoryCommand : Command
{
    public ArticleInventoryCommand(FargoApiClient client)
        : base("inventory", """
            Show the inventory count for an article.

            Pass --at to reconstruct the count as it was at a specific instant,
            using the item containment history, instead of the current count.

            Examples:
              fargo article inventory 11111111-1111-1111-1111-111111111111
              fargo article inventory 11111111-1111-1111-1111-111111111111 --include-descendants false
              fargo article inventory 11111111-1111-1111-1111-111111111111 --at 2026-03-01T12:00:00Z
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The article GUID."
        };

        var containersOption = new Option<Guid[]?>("--containers")
        {
            Description = "Limit inventory to these container item GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var includeDescendantsOption = new Option<bool>("--include-descendants")
        {
            Description = "Include items in nested containers. Defaults to true.",
            DefaultValueFactory = _ => true
        };

        var atOption = new Option<DateTimeOffset?>("--at")
        {
            Description = "Instant (ISO 8601, e.g. 2026-03-01T12:00:00Z) at which to evaluate the inventory. Defaults to now."
        };

        Add(guidArgument);
        Add(containersOption);
        Add(includeDescendantsOption);
        Add(atOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var containers = parseResult.GetValue(containersOption);
            var includeDescendants = parseResult.GetValue(includeDescendantsOption);
            var at = parseResult.GetValue(atOption);

            var result = await client.Articles[guid.ToString()].Inventory.GetAsync(request =>
            {
                request.QueryParameters.IncludeDescendents = includeDescendants;

                if (containers is { Length: > 0 })
                {
                    request.QueryParameters.InsideItemContainerGuids =
                        containers.Select(g => (Guid?)g).ToArray();
                }

                if (at.HasValue)
                {
                    request.QueryParameters.At = at.Value;
                }
            });

            if (result is null)
            {
                Console.Error.WriteLine("Article not found.");
                return 1;
            }

            var detail = new DetailWriter()
                .Title("Article inventory")
                .Field("Article", guid);

            if (at.HasValue)
            {
                detail.Field("As of", at.Value.ToString("u"));
            }

            detail
                .Field("Total count", result.TotalCount)
                .Write();

            return 0;
        });
    }
}
