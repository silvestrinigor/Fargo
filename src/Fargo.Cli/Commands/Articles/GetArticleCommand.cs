using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class GetArticleCommand : Command
{
    public GetArticleCommand(FargoApiClient client)
        : base("get", """
            Show the full details of a single article.

            Examples:
              fargo article get 11111111-1111-1111-1111-111111111111
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The article GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var article = await client.V1.Articles[guid.ToString()].GetAsync(cancellationToken: cancellationToken);

            if (article is null)
            {
                Console.Error.WriteLine("Article not found.");
                return 1;
            }

            ArticleDetailRenderer.Write(article);

            return 0;
        });
    }
}
