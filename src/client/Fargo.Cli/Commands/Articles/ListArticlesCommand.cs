using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ListArticlesCommand : Command
{
    public ListArticlesCommand(FargoApiClient client)
        : base("list", """
            List articles as a table.

            By default the list is paged interactively: a page of articles is
            shown and you can press 'n' for the next page, 'p' for the previous
            page, or 'q' to quit (press Enter after the key). Passing --page or
            --limit switches to a single, non-interactive fetch of exactly that
            page.

            Examples:
              fargo article list
              fargo article list --page 2
              fargo article list --page 1 --limit 50
              fargo article list --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1). Disables interactive paging."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of articles per page. Disables interactive paging."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show articles under these partition GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var sortOption = new Option<string?>("--sort")
        {
            Description = "Sort field, optionally prefixed with '-' for descending (e.g. name, -name). Fields: guid, name, description, type, trackingmode."
        };

        var searchOption = new Option<string?>("--search")
        {
            Description = "Case-insensitive text to match against the article name or description."
        };

        Add(pageOption);
        Add(limitOption);
        Add(partitionsOption);
        Add(sortOption);
        Add(searchOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);
            var partitions = parseResult.GetValue(partitionsOption);
            var sort = parseResult.GetValue(sortOption);
            var search = parseResult.GetValue(searchOption);

            await PagedListView.RunAsync<ArticleDto>(
                page,
                limit,
                fetch: async (p, l) =>
                {
                    var result = await client.V1.Articles.GetAsync(request =>
                    {
                        request.QueryParameters.Page = p;
                        request.QueryParameters.Limit = l;

                        if (partitions is { Length: > 0 })
                        {
                            request.QueryParameters.ChildOfAnyOfThesePartitions =
                                partitions.Select(g => (Guid?)g).ToArray();
                        }

                        if (!string.IsNullOrWhiteSpace(sort))
                        {
                            request.QueryParameters.Sort = sort;
                        }

                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            request.QueryParameters.Search = search;
                        }
                    }, cancellationToken);

                    return result is null
                        ? null
                        : new PageData<ArticleDto>(
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

    private static void Render(IReadOnlyList<ArticleDto> items)
    {
        new TableWriter<ArticleDto>()
            .Column("GUID", a => a.Guid?.ToString())
            .Column("NAME", a => a.Name)
            .Column("TYPE", a => EnumFormatting.Format<ArticleType>(a.ArticleType))
            .Column("DESCRIPTION", a => a.Description)
            .EmptyMessage("No articles found.")
            .Write(items);
    }
}
