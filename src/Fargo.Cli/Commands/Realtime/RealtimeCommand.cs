using Fargo.Cli.Configurations;
using Fargo.Client.Authentication;
using System.CommandLine;

namespace Fargo.Cli.Commands.Realtime;

public sealed class RealtimeCommand : Command
{
    public RealtimeCommand(
        IFargoCliConfigurationStore configurationStore,
        FargoTokenRefresher tokenRefresher,
        ITokenStore tokenStore)
        : base("realtime", "Watch changes pushed live by the Fargo real-time service.")
    {
        Add(new WatchRealtimeCommand(configurationStore, tokenRefresher, tokenStore));
    }
}
