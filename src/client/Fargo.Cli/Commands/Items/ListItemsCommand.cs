using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ListItemsCommand : Command
{
    public ListItemsCommand(FargoApiClient client)
        : base("list", """
            List items as a table.

            By default the list is paged interactively: a page of items is shown
            and you can press 'n' for the next page, 'p' for the previous page, or
            'q' to quit (press Enter after the key). Passing --page or --limit
            switches to a single, non-interactive fetch of exactly that page.

            Examples:
              fargo item list
              fargo item list --page 2
              fargo item list --page 1 --limit 50
              fargo item list --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1). Disables interactive paging."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of items per page. Disables interactive paging."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show items under these partition GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(pageOption);
        Add(limitOption);
        Add(partitionsOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);
            var partitions = parseResult.GetValue(partitionsOption);

            await PagedListView.RunAsync<ItemDto>(
                page,
                limit,
                fetch: async (p, l) =>
                {
                    var result = await client.V1.Items.GetAsync(request =>
                    {
                        request.QueryParameters.Page = p;
                        request.QueryParameters.Limit = l;

                        if (partitions is { Length: > 0 })
                        {
                            request.QueryParameters.ChildOfAnyOfThesePartitions =
                                partitions.Select(g => (Guid?)g).ToArray();
                        }
                    }, cancellationToken);

                    return result is null
                        ? null
                        : new PageData<ItemDto>(
                            result.Items ?? [],
                            result.TotalCount ?? 0,
                            result.Page ?? p,
                            result.TotalPages ?? 0,
                            result.HasNextPage ?? false,
                            result.HasPreviousPage ?? false);
                },
                renderer: Render,
                cancellationToken);

            return 0;
        });
    }

    private static void Render(IReadOnlyList<ItemDto> items)
    {
        new TableWriter<ItemDto>()
            .Column("GUID", i => i.Guid?.ToString())
            .Column("ARTICLE", i => i.ArticleGuid?.ToString())
            .Column("CONTAINER", i => i.ParentContainerGuid?.ToString())
            .Column("FIXED", i => i.IsFixed?.ToString())
            .EmptyMessage("No items found.")
            .Write(items);
    }
}
