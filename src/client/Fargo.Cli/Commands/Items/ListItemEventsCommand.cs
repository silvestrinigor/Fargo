using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ListItemEventsCommand : Command
{
    public ListItemEventsCommand(FargoApiClient client)
        : base("list", """
            List item events as a table.

            Item events record stock changes for event-tracked articles - for
            example a movement of a quantity between containers. Results can be
            filtered by article, event type, and time period. Requires the
            "read item events" permission; only events whose article you can
            access are returned.

            By default the list is paged interactively: a page of entries is shown
            and you can press 'n' for the next page, 'p' for the previous page, or
            'q' to quit (press Enter after the key). Passing --page or --limit
            switches to a single, non-interactive fetch of exactly that page.

            Examples:
              fargo item event list
              fargo item event list --article 11111111-1111-1111-1111-111111111111
              fargo item event list --from 2026-01-01T00:00:00Z --to 2026-02-01T00:00:00Z
              fargo item event list --page 1 --limit 50
            """)
    {
        var articleOption = new Option<Guid?>("--article")
        {
            Description = "Only show events for this event-tracked article GUID."
        };

        var typeOption = new Option<int?>("--type")
        {
            Description = "Only show events of this type (1 = moviment)."
        };

        var fromOption = new Option<DateTimeOffset?>("--from")
        {
            Description = "Only show events at or after this instant (ISO 8601 UTC)."
        };

        var toOption = new Option<DateTimeOffset?>("--to")
        {
            Description = "Only show events at or before this instant (ISO 8601 UTC)."
        };

        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1). Disables interactive paging."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of events per page. Disables interactive paging."
        };

        Add(articleOption);
        Add(typeOption);
        Add(fromOption);
        Add(toOption);
        Add(pageOption);
        Add(limitOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var article = parseResult.GetValue(articleOption);
            var type = parseResult.GetValue(typeOption);
            var from = parseResult.GetValue(fromOption);
            var to = parseResult.GetValue(toOption);
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);

            await PagedListView.RunAsync<ItemEventDto>(
                page,
                limit,
                fetch: async (p, l) =>
                {
                    var result = await client.ItemEvents.GetAsync(request =>
                    {
                        request.QueryParameters.Page = p;
                        request.QueryParameters.Limit = l;

                        if (article.HasValue)
                        {
                            request.QueryParameters.ArticleGuid = article.Value;
                        }

                        if (type.HasValue)
                        {
                            request.QueryParameters.EventType = type.Value;
                        }

                        if (from.HasValue)
                        {
                            request.QueryParameters.PeriodStart = from.Value;
                        }

                        if (to.HasValue)
                        {
                            request.QueryParameters.PeriodEnd = to.Value;
                        }
                    }, cancellationToken);

                    return result is null
                        ? null
                        : new PageData<ItemEventDto>(
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

    private static void Render(IReadOnlyList<ItemEventDto> items)
    {
        new TableWriter<ItemEventDto>()
            .Column("GUID", e => e.Guid?.ToString())
            .Column("ARTICLE", e => e.ArticleGuid?.ToString())
            .Column("TYPE", e => EnumFormatting.Format<ItemEventType>(e.EventType))
            .Column("QUANTITY", e => e.Moviment?.Quantity?.Double?.ToString())
            .Column("FROM", e => e.Moviment?.FromItemContainerGuid?.ToString())
            .Column("TO", e => e.Moviment?.ToItemContainerGuid?.ToString())
            .Column("OCCURRED AT", e => e.OccuredAt?.ToString("u"))
            .EmptyMessage("No item events found.")
            .Write(items);
    }
}
