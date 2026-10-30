using Fargo.Cli.Output;
using Fargo.ClientHttp;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class DeleteServiceAccountCommand : Command
{
    public DeleteServiceAccountCommand(FargoApiClient client)
        : base("delete", """
            Delete a service account by GUID. Any tokens it holds stop validating.

            Examples:
              fargo service-account delete 00000000-0000-0000-0000-000000000010
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The service account GUID."
        };

        Add(guidArgument);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            await client.V1.ServiceAccounts[guid].DeleteAsync(cancellationToken: cancellationToken);

            StatusWriter.Success("Service account deleted.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}