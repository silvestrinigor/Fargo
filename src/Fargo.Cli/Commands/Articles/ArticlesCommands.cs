using Fargo.Http.Client;
using System.CommandLine;

namespace Fargo.Cli.Commands.Articles;

public sealed class ArticlesCommand : Command
{
    public ArticlesCommand(FargoApiClient client)
        : base("articles", "Manage articles")
    {
        Add(new ListArticlesCommand(client));
    }
}
