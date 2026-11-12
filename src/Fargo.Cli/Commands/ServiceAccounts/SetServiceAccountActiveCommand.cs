using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class SetServiceAccountActiveCommand : Command
{
    public SetServiceAccountActiveCommand(FargoApiClient client)
        : base("set-active", """
            Activate or deactivate a service account.

            This is a separate operation from update and requires its own permission.

            Examples:
              fargo service-account set-active 00000000-0000-0000-0000-000000000010 true
              fargo service-account set-active 00000000-0000-0000-0000-000000000010 false
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The service account GUID."
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

            await client.V1.ServiceAccounts[guid].IsActive.PutAsync(
                new ActiveStateDto { IsActive = active },
                cancellationToken: cancellationToken);

            StatusWriter.Success(active ? "Service account activated." : "Service account deactivated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}