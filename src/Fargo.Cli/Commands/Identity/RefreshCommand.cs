using Fargo.Http.Client;
using Fargo.Http.Client.Authentication;
using Fargo.Http.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class RefreshCommand : Command
{
    public RefreshCommand(FargoApiClient client, ITokenStore tokenStore)
        : base("refresh", "Refresh the access token using the stored refresh token")
    {
        SetAction(async parseResult =>
        {
            var tokens = await tokenStore.GetAsync();

            if (tokens is null)
            {
                Console.Error.WriteLine("Not logged in. Run 'identity login' first.");
                return 1;
            }

            var result = await client.Identity.Refresh.PostAsync(new IdentityRefreshDto
            {
                RefreshToken = new UntypedString(tokens.RefreshToken)
            });

            if (result is null)
            {
                Console.Error.WriteLine("Token refresh failed.");
                return 1;
            }

            await tokenStore.SetAsync(new AuthTokens(
                result.AccessToken!,
                result.RefreshToken!,
                result.ExpiresAt!.Value));

            Console.WriteLine($"Token refreshed. Expires at: {result.ExpiresAt:O}");

            return 0;
        });
    }
}
