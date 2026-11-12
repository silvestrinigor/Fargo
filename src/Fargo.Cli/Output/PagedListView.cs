namespace Fargo.Cli.Output;

/// <summary>
/// One fetched page, normalized from an entity-specific paged-result envelope. The caller extracts it
/// because the generated <c>PagedResultOf*Dto</c> types share no common base type.
/// </summary>
public sealed record PageData<TItem>(
    IReadOnlyList<TItem> Items,
    int TotalCount,
    int Page,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

/// <summary>
/// Drives list output with paging: a single non-interactive fetch when the user pins a page/limit
/// (or input is redirected), or an interactive pager that reads 'n'/'p'/'q' to move between pages.
/// </summary>
public static class PagedListView
{
    public const int DefaultPageSize = 20;

    /// <param name="fetch">
    /// Given a 1-based page index and a limit, returns the page, or <see langword="null"/> when the server
    /// returned nothing.
    /// </param>
    public static async Task RunAsync<TItem>(
        int? page,
        int? limit,
        Func<int, int, Task<PageData<TItem>?>> fetch,
        Action<IReadOnlyList<TItem>> renderer,
        CancellationToken cancellationToken = default)
    {
        // A single, non-interactive fetch is used when the user pins a page or
        // limit, or when input is redirected (no interactive terminal to read
        // key presses from).
        var oneShot = page.HasValue || limit.HasValue || Console.IsInputRedirected;

        if (oneShot)
        {
            var result = await fetch(page ?? 1, limit ?? DefaultPageSize);
            renderer(result?.Items ?? []);
            PrintStaticFooter(result);
            return;
        }

        await RunInteractiveAsync(fetch, renderer, cancellationToken);
    }

    private static async Task RunInteractiveAsync<TItem>(
        Func<int, int, Task<PageData<TItem>?>> fetch,
        Action<IReadOnlyList<TItem>> renderer,
        CancellationToken cancellationToken)
    {
        var currentPage = 1;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await fetch(currentPage, DefaultPageSize);

            renderer(result?.Items ?? []);

            var totalCount = result?.TotalCount ?? 0;

            if (totalCount == 0)
            {
                // Nothing to page through.
                return;
            }

            var totalPages = result?.TotalPages ?? 0;

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

    private static void PrintStaticFooter<TItem>(PageData<TItem>? result)
    {
        if (result is null || result.TotalCount <= 0)
        {
            return;
        }

        Console.WriteLine($"Page {result.Page} of {result.TotalPages} ({result.TotalCount} total).");

        if (result.HasNextPage)
        {
            Console.WriteLine($"Use --page {result.Page + 1} to see the next page.");
        }
    }
}
