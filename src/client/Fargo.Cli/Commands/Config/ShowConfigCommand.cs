using Fargo.Cli.Configurations;
using Fargo.Cli.Output;
using System.CommandLine;

namespace Fargo.Cli.Commands.Config;

public sealed class ShowConfigCommand : Command
{
    public ShowConfigCommand(IFargoCliConfigurationStore configurationStore)
        : base("show", """
            Show the current CLI configuration.

            Examples:
              fargo config show
            """)
    {
        SetAction(parseResult =>
        {
            var configuration = configurationStore.Load();

            new DetailWriter()
                .Title("CLI configuration")
                .Field("Server", configuration.Server?.ToString())
                .Field("Credential ID", configuration.CredentialId ?? "(none)")
                .Write();

            return 0;
        });
    }
}
