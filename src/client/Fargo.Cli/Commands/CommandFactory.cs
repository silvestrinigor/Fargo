using Fargo.Cli.Commands.Articles;
using Fargo.Cli.Commands.Audits;
using Fargo.Cli.Commands.Config;
using Fargo.Cli.Commands.Identity;
using Fargo.Cli.Commands.Items;
using Fargo.Cli.Commands.Partitions;
using Fargo.Cli.Commands.UserGroups;
using Fargo.Cli.Commands.Users;
using Fargo.Cli.Configurations;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace Fargo.Cli.Commands;

public static class CommandFactory
{
    public static async Task<RootCommand> Create(
        IServiceProvider services,
        GlobalOptions globalOptions)
    {
        var root = new RootCommand("Fargo CLI");

        globalOptions.AddToRoot(root);

        var client = services.GetRequiredService<FargoApiClient>();

        var tokenStore = services.GetRequiredService<ITokenStore>();

        var configurationStore =
            services.GetRequiredService<IFargoCliConfigurationStore>();

        root.Add(new ConfigCommand(configurationStore));

        root.Add(new IdentityCommand(client, tokenStore));

        root.Add(new ArticlesCommand(client));

        root.Add(new ItemsCommand(client));

        root.Add(new PartitionsCommand(client));

        root.Add(new UserGroupsCommand(client));

        root.Add(new UsersCommand(client));

        root.Add(new AuditLogsCommand(client));

        return root;
    }
}
