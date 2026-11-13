using Fargo.Cli.Output;
using Fargo.Client;
using Fargo.Client.Models;
using System.CommandLine;

namespace Fargo.Cli.Commands.ServiceAccounts;

public sealed class UpdateServiceAccountCommand : Command
{
    public UpdateServiceAccountCommand(FargoApiClient client)
        : base("update", """
            Update the description of an existing service account.

            Activation, permissions and partitions have their own commands: set-active,
            set-permissions and set-partitions.

            Examples:
              fargo service-account update 00000000-0000-0000-0000-000000000010 --description "CI runner"
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The service account GUID."
        };

        var descriptionOption = new Option<string?>("--description")
        {
            Description = "New description."
        };

        Add(guidArgument);
        Add(descriptionOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var dto = new ServiceAccountUpdateDto
            {
                Description = parseResult.GetValue(descriptionOption)
            };

            await client.V1.ServiceAccounts[guid].PutAsync(dto, cancellationToken: cancellationToken);

            StatusWriter.Success("Service account updated.", ("GUID", guid.ToString()));

            return 0;
        });
    }
}
