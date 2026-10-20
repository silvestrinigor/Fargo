using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Audits;

public sealed class ListAuditLogsCommand : Command
{
    public ListAuditLogsCommand(FargoApiClient client)
        : base("list", """
            List audit log entries as a table, most useful with paging.

            Each row shows who (actor) did what (action) to which entity, and when.

            Examples:
              fargo audit list
              fargo audit list --page 1 --limit 50
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1)."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of audit entries per page."
        };

        Add(pageOption);
        Add(limitOption);

        SetAction(async parseResult =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);

            var result = await client.AuditLogs.GetAsync(request =>
            {
                if (page.HasValue)
                {
                    request.QueryParameters.Page = page.Value;
                }

                if (limit.HasValue)
                {
                    request.QueryParameters.Limit = limit.Value;
                }
            });

            new TableWriter<Fargo.ClientHttp.Models.AuditLogDto>()
                .Column("GUID", a => a.Guid?.ToString())
                .Column("ACTOR", a => a.ActorGuid?.ToString())
                .Column("ACTOR TYPE", a => a.ActorType?.ToString())
                .Column("ACTION", a => a.ActionType?.ToString())
                .Column("ENTITY TYPE", a => a.EntityType?.ToString())
                .Column("ENTITY", a => a.EntityGuid?.ToString())
                .Column("OCCURRED AT", a => a.OccurredAt?.ToString("u"))
                .EmptyMessage("No audit logs found.")
                .Write(result);

            return 0;
        });
    }
}
