using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Audits;

public sealed class ListAuditLogsCommand : Command
{
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

            await PagedListView.RunAsync<AuditLogDto>(
                page,
                limit,
                fetch: async (p, l) =>
                {
                    var result = await client.AuditLogs.GetAsync(request =>
                    {
                        request.QueryParameters.Page = p;
                        request.QueryParameters.Limit = l;
                    });

                    return result is null
                        ? null
                        : new PageData<AuditLogDto>(
                            result.Items ?? [],
                            result.TotalCount ?? 0,
                            result.Page ?? p,
                            result.TotalPages ?? 0,
                            result.HasNextPage ?? false,
                            result.HasPreviousPage ?? false);
                },
                renderer: Render);

            return 0;
        });
    }

    private static void Render(IReadOnlyList<AuditLogDto> items)
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
            .Write(items);
    }
}
