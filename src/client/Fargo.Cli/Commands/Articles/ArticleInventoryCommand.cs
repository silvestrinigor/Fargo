using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticleInventoryCommand : Command
{
    public ArticleInventoryCommand(FargoApiClient client)
        : base("inventory", "Get inventory count for an article")
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
            DefaultValueFactory = x => true
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
                    request.QueryParameters.InsideItemContainerGuids = containers.Select(g => (Guid?)g).ToArray();
                }
            });

            if (result is null)
            {
                Console.Error.WriteLine("Article not found.");
                return 1;
            }

            Console.WriteLine($"Article: {guid}");
            Console.WriteLine($"Total:   {result.TotalCount}");

            return 0;
        });
    }
}
