using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Items;

public sealed class ListItemsCommand : Command
{
    public ListItemsCommand(FargoApiClient client)
        : base("list", """
            List items as a table, with optional paging and partition filters.

            Examples:
              fargo item list
              fargo item list --page 1 --limit 20
              fargo item list --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1)."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of items per page."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show items under these partition GUIDs (space-separated).",
            AllowMultipleArgumentsPerToken = true
        };

        Add(pageOption);
        Add(limitOption);
        Add(partitionsOption);

        SetAction(async parseResult =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);
            var partitions = parseResult.GetValue(partitionsOption);

            var result = await client.Items.GetAsync(request =>
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

            new TableWriter<Fargo.ClientHttp.Models.ItemDto>()
                .Column("GUID", i => i.Guid?.ToString())
                .Column("ARTICLE", i => i.ArticleGuid?.ToString())
                .Column("CONTAINER", i => i.ParentContainerGuid?.ToString())
                .Column("FIXED", i => i.IsFixed?.ToString())
                .EmptyMessage("No items found.")
                .Write(result);

            return 0;
        });
    }
}
