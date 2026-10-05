using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ListItemsCommand : Command
{
    public ListItemsCommand(FargoApiClient client)
        : base("list", "List items")
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Number of items per page."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Filter by partition GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(pageOption);
        Add(limitOption);
        Add(partitionsOption);

        SetAction(async parseResult =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);
            var partitions = parseResult.GetValue(partitionsOption);

            var result = await client.Items.GetAsync(request =>
            {
                if (page.HasValue)
                {
                    request.QueryParameters.Page = page.Value;
                }

                if (limit.HasValue)
                {
                    request.QueryParameters.Limit = limit.Value;
                }

                if (partitions is { Length: > 0 })
                {
                    request.QueryParameters.ChildOfAnyOfThesePartitions = partitions.Select(g => (Guid?)g).ToArray();
                }
            });

            foreach (var item in result ?? [])
            {
                Console.WriteLine($"{item.Guid}  article={item.ArticleGuid}");
            }
        });
    }
}
