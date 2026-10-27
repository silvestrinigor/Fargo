using Fargo.Cli.Authentication;
using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Authentication;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Identity;

public sealed class LoginCommand : Command
{
    public LoginCommand(FargoApiClient client, ITokenStore tokenStore)
        : base("login", """
            Authenticate with the Fargo server and store the session token.

            If --password is omitted you will be prompted to type it securely
            (the input is not echoed).

            Examples:
              fargo identity login --username admin
              fargo identity login --username admin --password 'S3cr3t!'
            """)
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

        SetAction(async (parseResult, cancellationToken) =>
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

            var result = await client.V1.Identity.Login.PostAsync(
                new IdentityLoginDto
                {
                    Nameid = username,
                    Password = password
                }, cancellationToken: cancellationToken);

            if (result is null)
            {
                Console.Error.WriteLine("Login failed.");
                return 1;
            }

            await tokenStore.SetAsync(new AuthTokens(result.AccessToken!, result.RefreshToken!, result.ExpiresAt!.Value));

            StatusWriter.Success(
                "Login successful.",
                ("User", username));

            return 0;
        });
    }
}

