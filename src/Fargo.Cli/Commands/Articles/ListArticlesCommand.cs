using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ListArticlesCommand : Command
{
    public ListArticlesCommand(FargoApiClient client)
        : base("list", "List articles")
    {
        var pageOption = new Option<int?>("--page")
        {
            Description = "Page number."
        };

        var limitOption = new Option<int?>("--limit")
        {
            Description = "Number of articles per page."
        };

        Add(pageOption);
        Add(limitOption);

        SetAction(async parseResult =>
        {
            var page = parseResult.GetValue(pageOption);
            var limit = parseResult.GetValue(limitOption);

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
            });

            foreach (var article in result ?? [])
            {
                Console.WriteLine($"{article.Guid}  {article.Name}");
            }
        });
    }
}
