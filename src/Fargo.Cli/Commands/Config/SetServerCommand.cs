using Fargo.Cli.Configurations;
using Fargo.Cli.Output;
using System.CommandLine;

namespace Fargo.Cli.Commands.Config;

public sealed class SetServerCommand : Command
{
    public SetServerCommand(IFargoCliConfigurationStore configurationStore)
        : base("set-server", """
            Set and persist the Fargo server URL the CLI connects to by default.

            This is used when a command does not pass --server explicitly.

            Examples:
              fargo config set-server --url https://localhost:7563
            """)
    {
        var urlOption = new Option<string>("--url")
        {
            Description = "The Fargo server URL (e.g. https://localhost:7563).",
            Required = true
        };

        Add(urlOption);

        SetAction(parseResult =>
        {
            var url = parseResult.GetValue(urlOption)!;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var server)
                || (server.Scheme != Uri.UriSchemeHttp
                    && server.Scheme != Uri.UriSchemeHttps))
            {
                Console.Error.WriteLine(
                    $"Invalid server URL: '{url}'. Provide an absolute http or https URL.");
                return 1;
            }

            var configuration = configurationStore.Load();

            configuration.Server = server;

            configurationStore.Save(configuration);

            StatusWriter.Success("Server URL updated.", ("Server", server.ToString()));

            return 0;
        });
    }
}
