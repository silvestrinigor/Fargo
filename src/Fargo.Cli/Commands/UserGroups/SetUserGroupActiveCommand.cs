using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.UserGroups;

public sealed class SetUserGroupActiveCommand : Command
{
    public SetUserGroupActiveCommand(FargoApiClient client)
        : base("set-active", """
            Activate or deactivate a user group.

            This is a separate operation from update and requires its own permission.

            Examples:
              fargo user-group set-active 00000000-0000-0000-0000-000000000003 true
              fargo user-group set-active 00000000-0000-0000-0000-000000000003 false
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The user group GUID."
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

            await client.V1.UserGroups[guid].IsActive.PutAsync(
                new ActiveStateDto { IsActive = active },
                cancellationToken: cancellationToken);

            StatusWriter.Success(active ? "User group activated." : "User group deactivated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
