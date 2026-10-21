using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Partitions;

public sealed class ListPartitionsCommand : Command
{
    public ListPartitionsCommand(FargoApiClient client)
        : base("list", """
            List partitions as a table, with optional paging and parent filters.

            Examples:
              fargo partition list
              fargo partition list --page 1 --limit 20
              fargo partition list --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1)."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of partitions per page."
        };

        var parentOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show partitions under these parent GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(pageOption);
        Add(limitOption);
        Add(parentOption);

        SetAction(async parseResult =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);
            var partitions = parseResult.GetValue(parentOption);

            var result = await client.Partitions.GetAsync(request =>
            {
                if (page.HasValue)
                {
                    request.QueryParameters.Page = page.Value;
                }

                if (limit.HasValue)
                {
                    request.QueryParameters.Limit = limit.Value;
                }

                if (partitions is { Length: > 0 })
                {
                    request.QueryParameters.ChildOfAnyOfThesePartitions =
                        partitions.Select(g => (Guid?)g).ToArray();
                }
            });

            new TableWriter<Fargo.ClientHttp.Models.PartitionDto>()
                .Column("GUID", p => p.Guid?.ToString())
                .Column("NAME", p => p.Name)
                .Column("PARENT", p => p.ParentPartitionGuid?.ToString())
                .Column("GLOBAL", p => p.IsGlobalPartition?.ToString())
                .EmptyMessage("No partitions found.")
                .Write(result);

            return 0;
        });
    }
}
