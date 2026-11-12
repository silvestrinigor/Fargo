using Fargo.Cli.Configurations;
using System.CommandLine;

namespace Fargo.Cli.Commands.Config;

public sealed class GetRealtimeServerCommand : Command
{
    public GetRealtimeServerCommand(IFargoCliConfigurationStore configurationStore)
        : base("get-realtime-server", """
            Print the configured Fargo real-time service URL.

            Examples:
              fargo config get-realtime-server
            """)
    {
        SetAction(parseResult =>
        {
            var configuration = configurationStore.Load();

            if (configuration.RealtimeServer is null)
            {
                Console.Error.WriteLine(
                    "No real-time server is configured. Set one with "
                    + "'fargo config set-realtime-server --url <url>'.");
                return 1;
            }

            Console.WriteLine(configuration.RealtimeServer);

            return 0;
        });
    }
}
