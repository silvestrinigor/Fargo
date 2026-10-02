using Fargo.Http.Client;
using Fargo.Http.Client.Authentication;
using Fargo.Http.Client.Models;
using Microsoft.Kiota.Abstractions;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class LogoutCommand : Command
{
    public LogoutCommand(FargoApiClient client, ITokenStore tokenStore)
        : base("logout", "Log out from the Fargo server")
    {
        SetAction(async parseResult =>
        {
            var tokens = await tokenStore.GetAsync();

            if (tokens is null)
            {
                Console.Error.WriteLine("Not logged in.");
                return 1;
            }

            try
            {
                await client.Identity.Logout.PostAsync(new IdentityLogOutDto
                {
                    RefreshToken = new UntypedString(tokens.RefreshToken)
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Logout request failed: {ex.Message}");
                // Still clear local tokens
            }

            await tokenStore.ClearAsync();

            Console.WriteLine("Logged out successfully.");

            return 0;
        });
    }
}
