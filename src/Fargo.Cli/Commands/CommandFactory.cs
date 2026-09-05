using Fargo.Cli.Commands.Articles;
using Fargo.Http.Client;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace Fargo.Cli.Commands;

public static class CommandFactory
{
    public static RootCommand Create(IServiceProvider services)
    {
        var root = new RootCommand("Fargo CLI");

        var client = services.GetRequiredService<FargoApiClient>();

        root.Add(new ArticlesCommand(client));

        return root;
    }
}
