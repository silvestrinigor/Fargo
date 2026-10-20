using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticleInventoryCommand : Command
{
    public ArticleInventoryCommand(FargoApiClient client)
        : base("inventory", """
            Show the inventory count for an article.

            Examples:
              fargo articles inventory 11111111-1111-1111-1111-111111111111
              fargo articles inventory 11111111-1111-1111-1111-111111111111 --include-descendants false
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

        Add(guidArgument);
        Add(containersOption);
        Add(includeDescendantsOption);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var containers = parseResult.GetValue(containersOption);
            var includeDescendants = parseResult.GetValue(includeDescendantsOption);

            var result = await client.Articles[guid.ToString()].Inventory.GetAsync(request =>
            {
                request.QueryParameters.IncludeDescendents = includeDescendants;

                if (containers is { Length: > 0 })
                {
                    request.QueryParameters.InsideItemContainerGuids =
                        containers.Select(g => (Guid?)g).ToArray();
                }
            });

            if (result is null)
            {
                Console.Error.WriteLine("Article not found.");
                return 1;
            }

            new DetailWriter()
                .Title("Article inventory")
                .Field("Article", guid)
                .Field("Total count", result.TotalCount)
                .Write();

            return 0;
        });
    }
}
