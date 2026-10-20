using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class RefreshCommand : Command
{
    public RefreshCommand(FargoApiClient client, ITokenStore tokenStore)
        : base("refresh", """
            Refresh the access token using the stored refresh token.

            Examples:
              fargo identity refresh
            """)
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
                RefreshToken = tokens.RefreshToken
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

            StatusWriter.Success(
                "Token refreshed.",
                ("Expires at", result.ExpiresAt?.ToString("u")));

            return 0;
        });
    }
}
