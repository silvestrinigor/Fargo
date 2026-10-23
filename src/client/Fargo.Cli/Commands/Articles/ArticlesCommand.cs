using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticlesCommand : Command
{
    public ArticlesCommand(FargoApiClient client)
        : base("article", "Manage articles - the catalog of product definitions.")
    {
        Add(new ListArticlesCommand(client));
        Add(new TreeArticlesCommand(client));
        Add(new GetArticleCommand(client));
        Add(new GetArticleByBarcodeCommand(client));
        Add(new ArticleInventoryCommand(client));
        Add(new ArticleCreateCommand(client));
        Add(new DeleteArticleCommand(client));
        Add(new DeleteArticleByBarcodeCommand(client));
        Add(new UpdateArticleCommand(client));
        Add(new UpdateArticleByBarcodeCommand(client));
    }
}
