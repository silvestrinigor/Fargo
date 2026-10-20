using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class IdentityCommand : Command
{
    public IdentityCommand(FargoApiClient client, ITokenStore tokenStore)
        : base("identity", "Authenticate with the Fargo server (login, logout, refresh, change password).")
    {
        Add(new LoginCommand(client, tokenStore));
        Add(new LogoutCommand(client, tokenStore));
        Add(new RefreshCommand(client, tokenStore));
        Add(new ChangePasswordCommand(client));
    }
}
