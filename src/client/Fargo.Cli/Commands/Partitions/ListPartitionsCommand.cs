using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class ListPartitionsCommand : Command
{
    public ListPartitionsCommand(FargoApiClient client)
        : base("list", """
            List partitions as a table.

            By default the list is paged interactively: a page of partitions is
            shown and you can press 'n' for the next page, 'p' for the previous
            page, or 'q' to quit (press Enter after the key). Passing --page or
            --limit switches to a single, non-interactive fetch of exactly that
            page.

            Examples:
              fargo partition list
              fargo partition list --page 2
              fargo partition list --page 1 --limit 50
              fargo partition list --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1). Disables interactive paging."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of partitions per page. Disables interactive paging."
        };

        var parentOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show partitions under these parent GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var sortOption = new Option<string?>("--sort")
        {
            Description = "Sort field, optionally prefixed with '-' for descending (e.g. -name). Fields: guid, name, description."
        };

        var searchOption = new Option<string?>("--search")
        {
            Description = "Case-insensitive text to match against the partition name or description."
        };

        Add(pageOption);
        Add(limitOption);
        Add(sortOption);
        Add(searchOption);
        Add(parentOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);
            var sort = parseResult.GetValue(sortOption);
            var search = parseResult.GetValue(searchOption);
            var partitions = parseResult.GetValue(parentOption);

            await PagedListView.RunAsync<PartitionDto>(
                page,
                limit,
                fetch: async (p, l) =>
                {
                    var result = await client.V1.Partitions.GetAsync(request =>
                    {
                        request.QueryParameters.Page = p;
                        request.QueryParameters.Limit = l;

                        if (!string.IsNullOrWhiteSpace(sort))
                        {
                            request.QueryParameters.Sort = sort;
                        }

                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            request.QueryParameters.Search = search;
                        }

                        if (partitions is { Length: > 0 })
                        {
                            request.QueryParameters.ChildOfAnyOfThesePartitions =
                                partitions.Select(g => (Guid?)g).ToArray();
                        }
                    }, cancellationToken);

                    return result is null
                        ? null
                        : new PageData<PartitionDto>(
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

    private static void Render(IReadOnlyList<PartitionDto> items)
    {
        new TableWriter<PartitionDto>()
            .Column("GUID", p => p.Guid?.ToString())
            .Column("NAME", p => p.Name)
            .Column("PARENT", p => p.ParentPartitionGuid?.ToString())
            .Column("GLOBAL", p => p.IsGlobalPartition?.ToString())
            .EmptyMessage("No partitions found.")
            .Write(items);
    }
}
