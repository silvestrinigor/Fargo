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
        var root = new RootCommand("""
            Fargo CLI - command-line client for the Fargo inventory API.

            Common workflow:
              1. Point the CLI at a server:  fargo config set-server --url https://localhost:7563
              2. Authenticate:               fargo identity login --username admin
              3. Work with resources:        fargo article list, fargo item get <guid>, ...

            Global options:
              --server              Override the configured server (available on every command).
              --username/--password Authenticate for a single invocation (resource commands only,
                                    not the identity commands).

            Run any command with --help to see its options and examples.
            """);

        globalOptions.AddServerToRoot(root);

        // Expose `--verbose` on every command for detailed error diagnostics.
        globalOptions.AddVerboseToRoot(root);

        // Expose `fargo --version`, reporting the assembly's informational
        // version (set from the build's VersionPrefix/VersionSuffix).
        root.Options.Add(new VersionOption());

        var client = services.GetRequiredService<FargoApiClient>();

        var tokenStore = services.GetRequiredService<ITokenStore>();

        var configurationStore =
            services.GetRequiredService<IFargoCliConfigurationStore>();

        // The identity group is intentionally excluded from the ad-hoc
        // credential options: its commands take their own credentials or use the
        // stored session, so --username/--password are not applicable there.
        root.Add(new IdentityCommand(client, tokenStore));

        var credentialGroups = new Command[]
        {
            new ConfigCommand(configurationStore),
            new ArticlesCommand(client),
            new ItemsCommand(client),
            new PartitionsCommand(client),
            new UserGroupsCommand(client),
            new UsersCommand(client),
            new AuditLogsCommand(client),
        };

        foreach (var group in credentialGroups)
        {
            globalOptions.AddCredentialsTo(group);
            root.Add(group);
        }

        return root;
    }
}
