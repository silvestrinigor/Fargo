using System.CommandLine;
using Fargo.Http.Client;
using Fargo.Http.Client.Authentication;
using Fargo.Http.Client.Models;

namespace Fargo.Cli.Commands.Identity;

public sealed class LoginCommand : Command
{
    public LoginCommand(FargoApiClient client, ITokenStore tokenStore)
        : base("login", "Authenticate with the Fargo server")
    {
        var usernameOption = new Option<string>("--username")
        {
            Description = "Username.",
            Required = true
        };

        var passwordOption = new Option<string>("--password")
        {
            Description = "Password.",
            Required = true
        };

        Add(usernameOption);
        Add(passwordOption);

        SetAction(async parseResult =>
        {
            var username = parseResult.GetValue(usernameOption)!;
            var password = parseResult.GetValue(passwordOption)!;

            var result = await client.Identity.Login.PostAsync(
                new LoginDto
                {
                    Nameid = username,
                    Password = password
                });

            if (result is null)
            {
                Console.Error.WriteLine("Login failed.");
                return 1;
            }

            await tokenStore.SetAsync(new AuthTokens(result.AccessToken!, result.RefreshToken!, result.ExpiresAt!.Value));

            Console.WriteLine("Login successful.");

            return 0;
        });
    }
}

