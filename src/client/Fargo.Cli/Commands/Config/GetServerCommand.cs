using Fargo.Cli.Configurations;
using System.CommandLine;

namespace Fargo.Cli.Commands.Config;

public sealed class GetServerCommand : Command
{
    public GetServerCommand(IFargoCliConfigurationStore configurationStore)
        : base("get-server", "Show the Fargo server URL the CLI connects to")
    {
        SetAction(parseResult =>
        {
            var configuration = configurationStore.Load();

            Console.WriteLine(configuration.Server);

            return 0;
        });
    }
}
