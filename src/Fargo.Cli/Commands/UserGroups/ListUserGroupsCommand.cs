using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class ListUserGroupsCommand : Command
{
    public ListUserGroupsCommand(FargoApiClient client)
        : base("list", """
            List user groups as a table.

            By default the list is paged interactively: a page of user groups is
            shown and you can press 'n' for the next page, 'p' for the previous
            page, or 'q' to quit (press Enter after the key). Passing --page or
            --limit switches to a single, non-interactive fetch of exactly that
            page.

            Examples:
              fargo user-group list
              fargo user-group list --page 2
              fargo user-group list --page 1 --limit 50
              fargo user-group list --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1). Disables interactive paging."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of user groups per page. Disables interactive paging."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show user groups under these partition GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        var sortOption = new Option<string?>("--sort")
        {
            Description = "Sort field, optionally prefixed with '-' for descending (e.g. -name). Fields: guid, nameid, description, isactive."
        };

        var searchOption = new Option<string?>("--search")
        {
            Description = "Case-insensitive text to match against the user group nameid or description."
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
            var sort = parseResult.GetValue(sortOption);
            var search = parseResult.GetValue(searchOption);
            var partitions = parseResult.GetValue(partitionsOption);

            await PagedListView.RunAsync<UserGroupDto>(
                page,
                limit,
                fetch: async (p, l) =>
                {
                    var result = await client.V1.UserGroups.GetAsync(request =>
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
                        : new PageData<UserGroupDto>(
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

    private static void Render(IReadOnlyList<UserGroupDto> items)
    {
        new TableWriter<UserGroupDto>()
            .Column("GUID", g => g.Guid?.ToString())
            .Column("NAME", g => g.Nameid)
            .Column("ACTIVE", g => g.IsActive?.ToString())
            .Column("ADMIN", g => g.IsAdminUserGroup?.ToString())
            .EmptyMessage("No user groups found.")
            .Write(items);
    }
}
