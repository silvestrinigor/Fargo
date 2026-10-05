using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticlesCommand : Command
{
    public ArticlesCommand(FargoApiClient client)
        : base("articles", "Manage articles")
    {
        Add(new ListArticlesCommand(client));
        Add(new GetArticleCommand(client));
        Add(new GetArticleByBarcodeCommand(client));
        Add(new ArticleInventoryCommand(client));
        Add(new CreateArticleCommand(client));
        Add(new DeleteArticleCommand(client));
        Add(new UpdateArticleCommand(client));
    }
}
