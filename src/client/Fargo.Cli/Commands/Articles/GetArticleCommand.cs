using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class GetArticleCommand : Command
{
    public GetArticleCommand(FargoApiClient client)
        : base("get", "Get a single article by GUID")
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The article GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var article = await client.Articles[guid.ToString()].GetAsync();

            if (article is null)
            {
                Console.Error.WriteLine("Article not found.");
                return 1;
            }

            Console.WriteLine($"GUID:  {article.Guid}");
            Console.WriteLine($"Name:  {article.Name}");
            Console.WriteLine($"Type:  {article.ArticleType}");

            return 0;
        });
    }
}
