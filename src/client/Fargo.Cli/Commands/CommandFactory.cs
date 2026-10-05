using Fargo.Cli.Commands.Articles;
using Fargo.Cli.Commands.Identity;
using Fargo.Http.Client;
using Fargo.Http.Client.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace Fargo.Cli.Commands;

public static class CommandFactory
{
    public static async Task<RootCommand> Create(IServiceProvider services)
    {
        var root = new RootCommand("Fargo CLI");

        var client = services.GetRequiredService<FargoApiClient>();

        var tokenStore = services.GetRequiredService<ITokenStore>();

        root.Add(new IdentityCommand(client, tokenStore));

        root.Add(new ArticlesCommand(client));

        return root;
    }
}
