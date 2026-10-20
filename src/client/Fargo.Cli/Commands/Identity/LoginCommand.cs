using Fargo.Cli.Authentication;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Models;
using System.CommandLine;

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

        var passwordOption = new Option<string?>("--password")
        {
            Description =
                "Password. If omitted, you will be prompted to enter it securely.",
            Required = false
        };

        Add(usernameOption);
        Add(passwordOption);

        SetAction(async parseResult =>
        {
            var username = parseResult.GetValue(usernameOption)!;
            var password = parseResult.GetValue(passwordOption);

            if (string.IsNullOrEmpty(password))
            {
                password = PasswordPrompt.Read();
            }

            if (string.IsNullOrEmpty(password))
            {
                Console.Error.WriteLine("Password is required.");
                return 1;
            }

            var result = await client.Identity.Login.PostAsync(
                new IdentityLoginDto
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

