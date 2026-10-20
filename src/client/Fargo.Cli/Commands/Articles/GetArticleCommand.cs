using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class GetArticleCommand : Command
{
    public GetArticleCommand(FargoApiClient client)
        : base("get", """
            Show the full details of a single article.

            Examples:
              fargo articles get 11111111-1111-1111-1111-111111111111
            """)
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

            new DetailWriter()
                .Title("Article")
                .Field("GUID", article.Guid)
                .Field("Name", article.Name)
                .Field("Type", EnumFormatting.Format<ArticleType>(article.ArticleType))
                .Field("Description", article.Description)
                .Field("Shelf life", article.ShelfLife)
                .Write();

            return 0;
        });
    }
}
