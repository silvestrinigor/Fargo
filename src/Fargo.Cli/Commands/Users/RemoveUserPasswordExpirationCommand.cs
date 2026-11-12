using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class RemoveUserPasswordExpirationCommand : Command
{
    public RemoveUserPasswordExpirationCommand(FargoApiClient client)
        : base("remove-password-expiration", """
            Stop the passwords of a user from expiring by default.

            Requires the ChangeAnotherUserPassword permission.

            Examples:
              fargo user remove-password-expiration 00000000-0000-0000-0000-000000000004
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.V1.Users[guid].PasswordExpiration.DeleteAsync(cancellationToken: cancellationToken);

            StatusWriter.Success("User password expiration removed.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}