using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class SetUserPasswordExpirationCommand : Command
{
    public SetUserPasswordExpirationCommand(FargoApiClient client)
        : base("set-password-expiration", """
            Set how long the passwords of a user last by default.

            The period is an ISO-8601 duration. It applies the next time the password is
            changed; PT0S means it must be changed immediately. Requires the
            ChangeAnotherUserPassword permission.

            Examples:
              fargo user set-password-expiration 00000000-0000-0000-0000-000000000004 P90D
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        var periodArgument = new Argument<string>("period")
        {
            Description = "The expiration period as an ISO-8601 duration, e.g. P90D."
        };

        Add(guidArgument);
        Add(periodArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var period = parseResult.GetValue(periodArgument)!;

            await client.V1.Users[guid].PasswordExpiration.PutAsync(
                new PasswordExpirationDto { ExpirationPeriod = period },
                cancellationToken: cancellationToken);

            StatusWriter.Success("User password expiration set.", ("GUID", guid.ToString()), ("Period", period));

            return 0;
        });
    }
}
