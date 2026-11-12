using Fargo.Cli.Configurations;
using Fargo.Cli.Output;
using System.CommandLine;

namespace Fargo.Cli.Commands.Config;

public sealed class SetRealtimeServerCommand : Command
{
    public SetRealtimeServerCommand(IFargoCliConfigurationStore configurationStore)
        : base("set-realtime-server", """
            Set and persist the Fargo real-time service URL used by 'fargo realtime watch'.

            The real-time service is separate from the API, so it has its own URL.
            This is used when 'fargo realtime watch' does not pass --realtime-server.

            Examples:
              fargo config set-realtime-server --url https://localhost:7571
            """)
    {
        var urlOption = new Option<string>("--url")
        {
            Description = "The real-time service URL (e.g. https://localhost:7571).",
            Required = true
        };

        Add(urlOption);

        SetAction(parseResult =>
        {
            var url = parseResult.GetValue(urlOption)!;

            if (!RealtimeServerUrl.TryParse(url, out var server))
            {
                Console.Error.WriteLine(
                    $"Invalid real-time server URL: '{url}'. Provide an absolute http or https URL.");
                return 1;
            }

            var configuration = configurationStore.Load();

            configuration.RealtimeServer = server;

            configurationStore.Save(configuration);

            StatusWriter.Success("Real-time server URL updated.", ("Real-time server", server.ToString()));

            return 0;
        });
    }
}

/// <summary>
/// Validates real-time service URLs given on the command line.
/// </summary>
internal static class RealtimeServerUrl
{
    public static bool TryParse(string? value, out Uri server)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var parsed)
            && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
        {
            server = parsed;
            return true;
        }

        server = null!;
        return false;
    }
}
