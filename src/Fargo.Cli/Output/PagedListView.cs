namespace Fargo.Cli.Output;

/// <summary>
/// One fetched page, normalized from an entity-specific paged-result envelope
/// into the shape the pager needs. GUID/metadata extraction is done by the
/// caller because the generated <c>PagedResultOf*Dto</c> types share no common
/// base type.
/// </summary>
/// <typeparam name="TItem">The row type rendered in the table.</typeparam>
public sealed record PageData<TItem>(
    IReadOnlyList<TItem> Items,
    int TotalCount,
    int Page,
    int TotalPages,
    bool HasNextPage,
    bool HasPreviousPage);

/// <summary>
/// Drives list output with paging: a single non-interactive fetch when the user
/// pins a page/limit (or output is redirected), or an interactive pager that
/// reads 'n'/'p'/'q' to move between server pages. Shared by every
/// <c>&lt;entity&gt; list</c> command so they behave identically.
/// </summary>
public static class PagedListView
{
    /// <summary>Default page size used when the user does not pass --limit.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Runs the list view.
    /// </summary>
    /// <typeparam name="TItem">The row type.</typeparam>
    /// <param name="page">The explicit page from --page, or <see langword="null"/>.</param>
    /// <param name="limit">The explicit limit from --limit, or <see langword="null"/>.</param>
    /// <param name="fetch">
    /// Fetches a page: given a 1-based page index and a limit, returns the
    /// normalized page data (or <see langword="null"/> when the server returned
    /// nothing).
    /// </param>
    /// <param name="renderer">
    /// Writes a page of rows as a table. Called once per displayed page.
    /// </param>
    public static async Task RunAsync<TItem>(
        int? page,
        int? limit,
        Func<int, int, Task<PageData<TItem>?>> fetch,
        Action<IReadOnlyList<TItem>> renderer,
        CancellationToken cancellationToken = default)
    {
        // A single, non-interactive fetch is used when the user pins a page or
        // limit, or when output is redirected (no interactive terminal to read
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

    /// <summary>
    /// Prints the paging summary used by the one-shot (non-interactive) path.
    /// </summary>
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
