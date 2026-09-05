using Fargo.Http.Client;
using Fargo.Http.Client.Authentication;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class IdentityCommand : Command
{
    public IdentityCommand(FargoApiClient client, ITokenStore tokenStore)
        : base("identity", "Manage Fargo identity and authentication")
    {
        Add(new LoginCommand(client, tokenStore));
    }
}
