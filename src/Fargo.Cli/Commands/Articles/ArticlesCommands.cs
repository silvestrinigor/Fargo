using System.CommandLine;
using Fargo.Http.Client;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticlesCommand : Command
{
    public ArticlesCommand(FargoApiClient client)
        : base("articles", "Manage articles")
    {
        Add(new ListArticlesCommand(client));
    }
}
