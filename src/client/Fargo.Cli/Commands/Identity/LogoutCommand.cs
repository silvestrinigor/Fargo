using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class LogoutCommand : Command
{
    public LogoutCommand(FargoApiClient client, ITokenStore tokenStore)
        : base("logout", """
            Log out from the Fargo server and clear the stored session token.

            Examples:
              fargo identity logout
            """)
    {
        SetAction(async (parseResult, cancellationToken) =>
        {
            var tokens = await tokenStore.GetAsync(cancellationToken);

            if (tokens is null)
            {
                Console.Error.WriteLine("Not logged in.");
                return 1;
            }

            try
            {
                await client.Identity.Logout.PostAsync(new IdentityLogOutDto
                {
                    RefreshToken = tokens.RefreshToken
                }, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Logout request failed: {ex.Message}");
                // Still clear local tokens
            }

            await tokenStore.ClearAsync(cancellationToken);

            StatusWriter.Success("Logged out successfully.");

            return 0;
        });
    }
}
