using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Audits;

public sealed class ListAuditLogsCommand : Command
{
    public ListAuditLogsCommand(FargoApiClient client)
        : base("list", "List audit logs")
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Number of audit logs per page."
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

            foreach (var auditLogs in result ?? [])
            {
                Console.WriteLine($"{auditLogs.Guid}  actorGuid={auditLogs.ActorGuid}  actorType={auditLogs.ActorType} actionType={auditLogs.ActionType} entityType={auditLogs.EntityType}");
            }
        });
    }
}
