using Fargo.Cli.Configurations;
using System.CommandLine;

namespace Fargo.Cli.Commands.Config;

public sealed class ShowConfigCommand : Command
{
    public ShowConfigCommand(IFargoCliConfigurationStore configurationStore)
        : base("show", "Show the current Fargo CLI configuration")
    {
        SetAction(parseResult =>
        {
            var configuration = configurationStore.Load();

            Console.WriteLine($"Server:       {configuration.Server}");
            Console.WriteLine(
                $"CredentialId: {configuration.CredentialId ?? "(none)"}");

            return 0;
        });
    }
}
