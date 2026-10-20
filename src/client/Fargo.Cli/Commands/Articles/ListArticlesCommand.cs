using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ListArticlesCommand : Command
{
    public ListArticlesCommand(FargoApiClient client)
        : base("list", """
            List articles as a table, with optional paging and partition filters.

            Examples:
              fargo articles list
              fargo articles list --page 1 --limit 20
              fargo articles list --partitions 00000000-0000-0000-0000-000000000001
            """)
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number to retrieve (starts at 1)."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Maximum number of articles per page."
        };

        var partitionsOption = new Option<Guid[]?>("--partitions")
        {
            Description = "Only show articles under these partition GUIDs (space-separated).",
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

            var result = await client.Articles.GetAsync(request =>
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

            new TableWriter<Fargo.ClientHttp.Models.ArticleDto>()
                .Column("GUID", a => a.Guid?.ToString())
                .Column("NAME", a => a.Name)
                .Column("TYPE", a => a.ArticleType?.ToString())
                .Column("DESCRIPTION", a => a.Description)
                .EmptyMessage("No articles found.")
                .Write(result);

            return 0;
        });
    }
}
