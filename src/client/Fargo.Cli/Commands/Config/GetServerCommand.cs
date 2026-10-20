using Fargo.Cli.Configurations;
using System.CommandLine;

namespace Fargo.Cli.Commands.Config;

public sealed class GetServerCommand : Command
{
    public GetServerCommand(IFargoCliConfigurationStore configurationStore)
        : base("get-server", """
            Print the configured default Fargo server URL.

            Examples:
              fargo config get-server
            """)
    {
        SetAction(parseResult =>
        {
            var configuration = configurationStore.Load();

            Console.WriteLine(configuration.Server);

            return 0;
        });
    }
}
