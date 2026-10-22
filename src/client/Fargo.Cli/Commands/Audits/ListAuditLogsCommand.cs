using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Audits;

public sealed class ListAuditLogsCommand : Command
{
    /// <summary>Default page size used when the user does not pass --limit.</summary>
    private const int DefaultPageSize = 20;

    public ListAuditLogsCommand(FargoApiClient client)
        : base("list", """
            List audit log entries as a table.

            Each row shows who (actor) did what (action) to which entity, and when.

            By default the list is paged interactively: a page of entries is shown
            and you can press 'n' for the next page, 'p' for the previous page, or
            'q' to quit (press Enter after the key). Passing --page or --limit
            switches to a single, non-interactive fetch of exactly that page.

            Examples:
              fargo audit list
              fargo audit list --page 2
              fargo audit list --page 1 --limit 50
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1). Disables interactive paging."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of audit entries per page. Disables interactive paging."
        };

        Add(pageOption);
        Add(limitOption);

        SetAction(async parseResult =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);

            // A single, non-interactive fetch is used when the user pins a page
            // or limit, or when output is redirected (no interactive terminal to
            // read key presses from).
            var oneShot = page.HasValue || limit.HasValue || Console.IsInputRedirected;

            if (oneShot)
            {
                var result = await FetchAsync(client, page ?? 1, limit ?? DefaultPageSize);
                Render(result);
                PrintStaticFooter(result);
                return 0;
            }

            await RunInteractiveAsync(client);
            return 0;
        });
    }

    /// <summary>
    /// Runs the interactive pager: shows a page, then reads n/p/q to move between
    /// pages, re-fetching each time. Stops when the user quits.
    /// </summary>
    private static async Task RunInteractiveAsync(FargoApiClient client)
    {
        var currentPage = 1;

        while (true)
        {
            var result = await FetchAsync(client, currentPage, DefaultPageSize);

            Render(result);

            var totalPages = result?.TotalPages ?? 0;
            var totalCount = result?.TotalCount ?? 0;

            if (totalCount == 0)
            {
                // Nothing to page through.
                return;
            }

            Console.WriteLine($"Page {currentPage} of {totalPages} ({totalCount} total).");

            var hasNext = result?.HasNextPage == true;
            var hasPrevious = result?.HasPreviousPage == true;

            var prompt = BuildPrompt(hasNext, hasPrevious);

            if (prompt is null)
            {
                // Only one page: nothing to navigate.
                return;
            }

            Console.Write(prompt);

            var input = Console.ReadLine();

            // End of input (Ctrl+Z/Ctrl+D) or an explicit quit.
            if (input is null)
            {
                Console.WriteLine();
                return;
            }

            var command = input.Trim().ToLowerInvariant();

            switch (command)
            {
                case "n" when hasNext:
                    currentPage++;
                    break;

                case "p" when hasPrevious:
                    currentPage--;
                    break;

                case "q":
                case "":
                    return;

                default:
                    Console.WriteLine("Unrecognized key. Use 'n', 'p', or 'q'.");
                    break;
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Builds the navigation prompt based on which moves are available, or
    /// <see langword="null"/> when there is nothing to navigate (single page).
    /// </summary>
    private static string? BuildPrompt(bool hasNext, bool hasPrevious)
    {
        var options = new List<string>();

        if (hasNext)
        {
            options.Add("'n' next");
        }

        if (hasPrevious)
        {
            options.Add("'p' previous");
        }

        if (options.Count == 0)
        {
            return null;
        }

        options.Add("'q' quit");

        return $"[{string.Join(", ", options)}]: ";
    }

    private static async Task<PagedResultOfAuditLogDto?> FetchAsync(
        FargoApiClient client, int page, int limit)
    {
        return await client.AuditLogs.GetAsync(request =>
        {
            request.QueryParameters.Page = page;
            request.QueryParameters.Limit = limit;
        });
    }

    private static void Render(PagedResultOfAuditLogDto? result)
    {
        new TableWriter<AuditLogDto>()
            .Column("GUID", a => a.Guid?.ToString())
            .Column("ACTOR", a => a.ActorGuid?.ToString())
            .Column("ACTOR TYPE", a => EnumFormatting.Format<ActorType>(a.ActorType))
            .Column("TYPE", a => EnumFormatting.Format<AuditLogType>(a.AuditLogType))
            .Column("ENTITY TYPE", a => EnumFormatting.Format<EntityType>(a.EntityType))
            .Column("ENTITY", a => a.EntityGuid?.ToString())
            .Column("OCCURRED AT", a => a.OccurredAt?.ToString("u"))
            .EmptyMessage("No audit logs found.")
            .Write(result?.Items);
    }

    /// <summary>
    /// Prints the paging summary used by the one-shot (non-interactive) path.
    /// </summary>
    private static void PrintStaticFooter(PagedResultOfAuditLogDto? result)
    {
        if (result is null || result.TotalCount is not > 0)
        {
            return;
        }

        var currentPage = result.Page ?? 1;
        var totalPages = result.TotalPages ?? 0;

        Console.WriteLine($"Page {currentPage} of {totalPages} ({result.TotalCount} total).");

        if (result.HasNextPage == true)
        {
            Console.WriteLine($"Use --page {currentPage + 1} to see the next page.");
        }
    }
}
