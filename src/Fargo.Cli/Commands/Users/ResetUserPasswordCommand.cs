using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class ResetUserPasswordCommand : Command
{
    public ResetUserPasswordCommand(FargoApiClient client)
        : base("reset-password", """
            Reset the password of a user.

            The user must choose a new password at the next sign-in. You cannot reset
            your own password this way; use 'identity change-password' instead.
            Requires the ChangeAnotherUserPassword permission.

            Examples:
              fargo user reset-password 00000000-0000-0000-0000-000000000004 --password 'Temp-S3cr3t!'
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        var passwordOption = new Option<string>("--password")
        {
            Description = "The new temporary password.",
            Required = true
        };

        Add(guidArgument);
        Add(passwordOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var password = parseResult.GetValue(passwordOption)!;

            await client.V1.Users[guid].Password.PutAsync(
                new PasswordResetDto { Password = password },
                cancellationToken: cancellationToken);

            StatusWriter.Success("User password reset.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}