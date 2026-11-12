using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class ListServiceAccountsCommand : Command
{
    public ListServiceAccountsCommand(FargoApiClient client)
        : base("list", """
            List service accounts as a table.

            By default the list is paged interactively: a page is shown and you
            can press 'n' for the next page, 'p' for the previous page, or 'q' to
            quit (press Enter after the key). Passing --page or --limit switches
            to a single, non-interactive fetch of exactly that page.

            Examples:
              fargo service-account list
              fargo service-account list --page 1 --limit 50
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1). Disables interactive paging."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of service accounts per page. Disables interactive paging."
        };

        Add(pageOption);
        Add(limitOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);

            await PagedListView.RunAsync<ServiceAccountDto>(
                page,
                limit,
                fetch: async (p, l) =>
                {
                    var result = await client.V1.ServiceAccounts.GetAsync(request =>
                    {
                        request.QueryParameters.Page = p;
                        request.QueryParameters.Limit = l;
                    }, cancellationToken);

                    return result is null
                        ? null
                        : new PageData<ServiceAccountDto>(
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

    private static void Render(IReadOnlyList<ServiceAccountDto> items)
    {
        new TableWriter<ServiceAccountDto>()
            .Column("GUID", a => a.Guid?.ToString())
            .Column("CLIENT ID", a => a.ClientId)
            .Column("ACTIVE", a => a.IsActive?.ToString())
            .EmptyMessage("No service accounts found.")
            .Write(items);
    }
}
