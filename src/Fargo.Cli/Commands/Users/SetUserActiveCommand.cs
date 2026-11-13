using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.Users;

public sealed class SetUserActiveCommand : Command
{
    public SetUserActiveCommand(FargoApiClient client)
        : base("set-active", """
            Activate or deactivate a user.

            This is a separate operation from update and requires its own permission.

            Examples:
              fargo user set-active 00000000-0000-0000-0000-000000000004 true
              fargo user set-active 00000000-0000-0000-0000-000000000004 false
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user GUID."
        };

        var activeArgument = new Argument<bool>("active")
        {
            Description = "true to activate, false to deactivate."
        };

        Add(guidArgument);
        Add(activeArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);
            var active = parseResult.GetValue(activeArgument);

            await client.V1.Users[guid].IsActive.PutAsync(
                new ActiveStateDto { IsActive = active },
                cancellationToken: cancellationToken);

            StatusWriter.Success(active ? "User activated." : "User deactivated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
