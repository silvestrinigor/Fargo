using Fargo.Cli.Configurations;
using System.CommandLine;

namespace Fargo.Cli.Commands.Config;

public sealed class ConfigCommand : Command
{
    public ConfigCommand(IFargoCliConfigurationStore configurationStore)
        : base("config", "Manage CLI configuration (the server URL this CLI connects to).")
    {
        Add(new SetServerCommand(configurationStore));
        Add(new GetServerCommand(configurationStore));
        Add(new ShowConfigCommand(configurationStore));
    }
}
